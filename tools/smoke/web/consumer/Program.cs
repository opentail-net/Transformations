using System.Data;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text.Json;
using Transformations;

namespace Smoke.Web;

// Usage: dotnet run -- --results <path.json>
// Hosts a small ASP.NET Core app in-process. Its endpoints call the Transformations.Web helpers, and each check drives an
// endpoint over HTTP and asserts what the helper did. Exit 0 when every check passed.
internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        string? resultsPath = ReadOption(args, "--results");
        int port = FreePort();
        string baseUrl = $"http://127.0.0.1:{port}";

        var builder = WebApplication.CreateBuilder(args);
        builder.WebHost.UseUrls(baseUrl);
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddSession(options => options.Cookie.Name = "smoke.session");
        builder.Services.AddScoped<ICookieHelper, CookieHelper>();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["App:Name"] = "Smoke",
            ["App:Retries"] = "3",
            ["ConnectionStrings:Main"] = "Server=example;Database=smoke",
        });

        WebApplication app = builder.Build();
        app.UseSession();
        MapEndpoints(app);
        await app.StartAsync();

        var checks = new List<(string Name, Func<HttpClient, Task> Run)>
        {
            ("QueryStringHelper: TryGetQuery reads an integer", QueryReadsInteger),
            ("QueryStringHelper: TryGetQuery rejects a non-integer", QueryRejectsNonInteger),
            ("CookieHelper: Set writes a cookie that Get reads back", CookieRoundTrip),
            ("CookieHelper: Set always marks the cookie Secure (over HTTP, a cookie-container client drops it)", CookieIsAlwaysSecure),
            ("CookieHelper: Delete expires the cookie", CookieDeleteExpires),
            ("SessionHelper: SetValue persists across requests", SessionPersists),
            ("WebHelper: Redirect with a format writes 302 and the formatted Location", RedirectFormatsLocation),
            ("WebHelper: SetFileNotFound writes 404", NotFoundIs404),
            ("WebHelper: SetStatus writes the given code", SetStatusWritesCode),
            ("ConfigurationHelper: GetSetting, GetValue, fallback and GetConnectionString", ConfigurationHelperReads),
            ("SelectListExtensions: ToSelectList marks the selected row", SelectListMarksSelected),
            ("ResponseHelper: RedirectAsync writes a script with the URL and window target", WindowRedirectWritesScript),
        };

        var results = new List<CheckResult>();
        try
        {
            foreach (var (name, run) in checks)
            {
                using var handler = new HttpClientHandler { AllowAutoRedirect = false, UseCookies = true, CookieContainer = new CookieContainer() };
                using var client = new HttpClient(handler) { BaseAddress = new Uri(baseUrl) };

                var clock = System.Diagnostics.Stopwatch.StartNew();
                bool passed = true;
                string detail = string.Empty;
                try
                {
                    await run(client);
                }
                catch (Exception ex)
                {
                    passed = false;
                    detail = $"{ex.GetType().Name}: {ex.Message}";
                }

                results.Add(new CheckResult(name, passed, detail, clock.ElapsedMilliseconds));
                Console.WriteLine(passed ? $"PASS {name}" : $"FAIL {name} - {detail}");
            }
        }
        finally
        {
            await app.StopAsync();
        }

        int failed = results.Count(r => !r.Passed);
        Console.WriteLine($"{results.Count - failed}/{results.Count} checks passed on {RuntimeInformation.FrameworkDescription}");

        if (resultsPath is not null)
        {
            File.WriteAllText(resultsPath, JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true }));
        }

        return failed == 0 ? 0 : 1;
    }

    // ── Endpoints: each one calls the helper under test ──────────────────────────────────────────────────────────────

    private static void MapEndpoints(WebApplication app)
    {
        app.MapGet("/query", (HttpContext context) =>
        {
            bool ok = QueryStringHelper.TryGetQuery<int>(context, "n", out int? n);
            return Results.Json(new
            {
                ok,
                n,
                has = QueryStringHelper.HasValue(context, "n"),
                all = QueryStringHelper.GetAllQueryStrings(context),
            });
        });

        app.MapGet("/cookie/set", (ICookieHelper cookies, string v) =>
        {
            cookies.Set("pref", v, 1);
            return Results.Ok();
        });

        app.MapGet("/cookie/read", (ICookieHelper cookies) => Results.Json(new { value = cookies.Get("pref"), exists = cookies.Exists("pref") }));

        app.MapGet("/cookie/delete", (ICookieHelper cookies) =>
        {
            cookies.Delete("pref");
            return Results.Ok();
        });

        app.MapGet("/session/set", (HttpContext context, string v) =>
        {
            SessionHelper.SetValue(context, "user", v);
            return Results.Ok();
        });

        app.MapGet("/session/get", (HttpContext context) => Results.Json(new
        {
            value = SessionHelper.GetValue<string>(context, "user"),
            exists = SessionHelper.Exists(context, "user"),
        }));

        app.MapGet("/redirect", (HttpContext context) =>
        {
            context.Response.Redirect("/items/{0}", 5);
            return Task.CompletedTask;
        });

        app.MapGet("/notfound", (HttpContext context) =>
        {
            context.Response.SetFileNotFound();
            return Task.CompletedTask;
        });

        app.MapGet("/status/{code:int}", (HttpContext context, int code) =>
        {
            context.Response.SetStatus(code);
            return Task.CompletedTask;
        });

        app.MapGet("/config", (IConfiguration configuration) => Results.Json(new
        {
            name = ConfigurationHelper.GetSetting(configuration, "App:Name"),
            retries = ConfigurationHelper.GetValue<int>(configuration, "App:Retries"),
            missing = ConfigurationHelper.GetSetting(configuration, "App:Missing", "fallback"),
            connection = ConfigurationHelper.GetConnectionString(configuration, "Main"),
        }));

        app.MapGet("/select", () =>
        {
            var table = new DataTable();
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Code", typeof(string));
            table.Rows.Add("Alpha", "a");
            table.Rows.Add("Beta", "b");

            return Results.Json(table.ToSelectList("Name", "Code", "b")
                .Select(item => new { item.Text, item.Value, item.Selected }));
        });

        app.MapGet("/window", async (HttpContext context) =>
        {
            await context.Response.RedirectAsync("/target", "_blank", "width=400");
        });
    }

    // ── Checks ──────────────────────────────────────────────────────────────────────────────────────────────────────

    private static async Task QueryReadsInteger(HttpClient client)
    {
        using JsonDocument doc = await GetJson(client, "/query?n=42");

        Expect(doc.RootElement.GetProperty("ok").GetBoolean(), "TryGetQuery returns true for 42");
        Expect(doc.RootElement.GetProperty("n").GetInt32() == 42, "the value is 42");
        Expect(doc.RootElement.GetProperty("has").GetBoolean(), "HasValue is true");
        Expect(doc.RootElement.GetProperty("all").GetProperty("n").GetString() == "42", "GetAllQueryStrings includes n=42");
    }

    private static async Task QueryRejectsNonInteger(HttpClient client)
    {
        using JsonDocument doc = await GetJson(client, "/query?n=abc");

        Expect(!doc.RootElement.GetProperty("ok").GetBoolean(), "TryGetQuery returns false for abc");
        Expect(doc.RootElement.GetProperty("has").GetBoolean(), "HasValue is still true for a present but non-integer key");
    }

    private static async Task CookieRoundTrip(HttpClient client)
    {
        HttpResponseMessage set = await client.GetAsync("/cookie/set?v=blue");
        Expect(set.Headers.TryGetValues("Set-Cookie", out var setCookies) && setCookies.Any(c => c.StartsWith("pref=blue")),
            "the response sets pref=blue");

        // A cookie-container client drops a Secure cookie over HTTP, so the value is sent back explicitly. That is what a
        // browser on HTTPS would do.
        var request = new HttpRequestMessage(HttpMethod.Get, "/cookie/read");
        request.Headers.Add("Cookie", "pref=blue");
        using JsonDocument doc = await ReadJson(await client.SendAsync(request));

        Expect(doc.RootElement.GetProperty("value").GetString() == "blue", "Get reads blue back from the request");
        Expect(doc.RootElement.GetProperty("exists").GetBoolean(), "Exists is true");
    }

    private static async Task CookieIsAlwaysSecure(HttpClient client)
    {
        HttpResponseMessage set = await client.GetAsync("/cookie/set?v=blue");
        string cookie = set.Headers.GetValues("Set-Cookie").Single(c => c.StartsWith("pref="));

        Expect(cookie.Contains("secure", StringComparison.OrdinalIgnoreCase), $"Set-Cookie has the Secure attribute (was {cookie})");
    }

    private static async Task CookieDeleteExpires(HttpClient client)
    {
        HttpResponseMessage response = await client.GetAsync("/cookie/delete");
        string cookie = response.Headers.GetValues("Set-Cookie").Single(c => c.StartsWith("pref="));

        Expect(cookie.Contains("expires=", StringComparison.OrdinalIgnoreCase), $"the deletion cookie carries an expiry (was {cookie})");
    }

    private static async Task SessionPersists(HttpClient client)
    {
        // The session cookie is not Secure, so the cookie container keeps it across requests over HTTP.
        await client.GetAsync("/session/set?v=ada");
        using JsonDocument doc = await GetJson(client, "/session/get");

        Expect(doc.RootElement.GetProperty("value").GetString() == "ada", "SessionHelper.GetValue returns ada on the next request");
        Expect(doc.RootElement.GetProperty("exists").GetBoolean(), "Exists is true");
    }

    private static async Task RedirectFormatsLocation(HttpClient client)
    {
        HttpResponseMessage response = await client.GetAsync("/redirect");

        Expect(response.StatusCode == HttpStatusCode.Redirect, $"302 Found (was {(int)response.StatusCode})");
        Expect(response.Headers.Location?.ToString() == "/items/5", $"Location /items/5 (was {response.Headers.Location})");
    }

    private static async Task NotFoundIs404(HttpClient client)
    {
        HttpResponseMessage response = await client.GetAsync("/notfound");

        Expect(response.StatusCode == HttpStatusCode.NotFound, $"404 (was {(int)response.StatusCode})");
    }

    private static async Task SetStatusWritesCode(HttpClient client)
    {
        HttpResponseMessage response = await client.GetAsync("/status/418");

        Expect((int)response.StatusCode == 418, $"418 (was {(int)response.StatusCode})");
    }

    private static async Task ConfigurationHelperReads(HttpClient client)
    {
        using JsonDocument doc = await GetJson(client, "/config");
        JsonElement root = doc.RootElement;

        Expect(root.GetProperty("name").GetString() == "Smoke", $"GetSetting reads App:Name (was {root.GetProperty("name")})");
        Expect(root.GetProperty("retries").GetInt32() == 3, $"GetValue<int> reads App:Retries as 3 (was {root.GetProperty("retries")})");
        Expect(root.GetProperty("missing").GetString() == "fallback", $"GetSetting uses the fallback for a missing key (was {root.GetProperty("missing")})");
        Expect(root.GetProperty("connection").GetString() == "Server=example;Database=smoke", "GetConnectionString reads Main");
    }

    private static async Task SelectListMarksSelected(HttpClient client)
    {
        using JsonDocument doc = await GetJson(client, "/select");
        JsonElement items = doc.RootElement;

        Expect(items.GetArrayLength() == 2, $"two items (was {items.GetArrayLength()})");
        // Results.Json uses web defaults, so the property names are camelCase on the wire.
        Expect(items[1].GetProperty("text").GetString() == "Beta" && items[1].GetProperty("selected").GetBoolean(),
            "Beta, the row with value b, is selected");
        Expect(!items[0].GetProperty("selected").GetBoolean(), "Alpha is not selected");
    }

    private static async Task WindowRedirectWritesScript(HttpClient client)
    {
        string body = await (await client.GetAsync("/window")).Content.ReadAsStringAsync();

        Expect(body.Contains("/target", StringComparison.Ordinal), "the script contains the URL");
        Expect(body.Contains("_blank", StringComparison.Ordinal), "the script contains the window target");
    }

    // ── Helpers ─────────────────────────────────────────────────────────────────────────────────────────────────────

    private static async Task<JsonDocument> GetJson(HttpClient client, string path) => await ReadJson(await client.GetAsync(path));

    private static async Task<JsonDocument> ReadJson(HttpResponseMessage response)
    {
        Expect(response.IsSuccessStatusCode, $"success status (was {(int)response.StatusCode})");
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync());
    }

    private static int FreePort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    private static void Expect(bool condition, string label)
    {
        if (!condition)
        {
            throw new InvalidOperationException(label);
        }
    }

    private static string? ReadOption(string[] args, string name)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == name)
            {
                return args[i + 1];
            }
        }

        return null;
    }
}

internal sealed record CheckResult(string Name, bool Passed, string Detail, long ElapsedMs);
