namespace Transformations.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    using Microsoft.AspNetCore.Http;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Configuration;

    using NUnit.Framework;

    [TestFixture]
    public class InfrastructureHelpersCoverageTests
    {
        [Test]
        public void ConfigurationHelper_EndToEnd_CoversAllPublicMethods()
        {
            var data = new Dictionary<string, string?>
            {
                ["App:Name"] = "Transformations",
                ["Number"] = "42",
                ["ConnectionStrings:Default"] = "Server=(localdb)\\MSSQLLocalDB;Database=Test;Trusted_Connection=True;",
            };

            IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection(data).Build();

            Assert.That(ConfigurationHelper.ContainsKey(config, "App:Name"), Is.True);
            Assert.That(ConfigurationHelper.ContainsKey(config, "Missing"), Is.False);
            Assert.That(ConfigurationHelper.ValueIsEmpty(config, "Missing"), Is.True);
            Assert.That(ConfigurationHelper.GetSetting(config, "App:Name"), Is.EqualTo("Transformations"));
            Assert.That(ConfigurationHelper.GetSetting(config, "Missing", "fallback"), Is.EqualTo("fallback"));
            Assert.That(ConfigurationHelper.GetValue<int>(config, "Number", -1), Is.EqualTo(42));
            Assert.That(ConfigurationHelper.GetValue<int>(config, "BadNumber", -1), Is.EqualTo(-1));
            Assert.That(ConfigurationHelper.GetConnectionString(config, "Default"), Does.Contain("Database=Test"));
            Assert.That(ConfigurationHelper.GetManualSetting(config, "App", "Name"), Is.EqualTo("Transformations"));

            bool found = ConfigurationHelper.TryGetSetting(config, "App:Name", out string result, "x");
            bool missing = ConfigurationHelper.TryGetSetting(config, "Missing", out string missingResult, "x");

            Assert.That(found, Is.True);
            Assert.That(result, Is.EqualTo("Transformations"));
            Assert.That(missing, Is.False);
            Assert.That(missingResult, Is.EqualTo("x"));
        }

        [Test]
        public void QueryStringHelper_CoversAllPublicMethods()
        {
            // Legacy contract: helper keeps first value in GetAllQueryStrings but "last wins" in ParseQueryString / TryGetQuery.
            var context = new DefaultHttpContext();
            context.Request.Scheme = "https";
            context.Request.Host = new HostString("example.com");
            context.Request.Path = "/path";
            context.Request.QueryString = new QueryString("?id=123&name=John&id=456");

            var all = QueryStringHelper.GetAllQueryStrings(context);
            var parsed = QueryStringHelper.ParseQueryString("https://example.com/path?id=123&name=John&id=456");
            var parsedRaw = QueryStringHelper.ParseQueryString("id=123&name=John");

            bool tryId = QueryStringHelper.TryGetQuery<int>(context, "id", out int? idValue);
            bool tryMissing = QueryStringHelper.TryGetQuery<int>(context, "missing", out int? missingValue);

            Assert.That(all["id"], Is.EqualTo("123,456"));
            Assert.That(parsed["id"], Is.EqualTo("456"));
            Assert.That(parsed["name"], Is.EqualTo("John"));
            Assert.That(parsedRaw["id"], Is.EqualTo("123"));
            Assert.That(tryId, Is.True);
            Assert.That(idValue, Is.EqualTo(456));
            Assert.That(tryMissing, Is.False);
            Assert.That(missingValue, Is.Null);
            Assert.That(QueryStringHelper.HasValue(context, "name"), Is.True);
            Assert.That(QueryStringHelper.HasValue(context, "missing"), Is.False);
        }

        [Test]
        public void WebHelper_CoversRedirectReloadAndStatusHelpers()
        {
            var context = new DefaultHttpContext();
            context.Request.Scheme = "https";
            context.Request.Host = new HostString("example.com");
            context.Request.Path = "/here";
            context.Request.QueryString = new QueryString("?x=1");

            HttpResponse response = context.Response;

            WebHelper.Redirect(response, "/item/{0}", 42);
            Assert.That(response.StatusCode, Is.EqualTo(StatusCodes.Status302Found));
            Assert.That(response.Headers["Location"].ToString(), Is.EqualTo("/item/42"));

            WebHelper.Reload(response);
            Assert.That(response.Headers["Location"].ToString(), Is.EqualTo("https://example.com/here?x=1"));

            WebHelper.SetFileNotFound(response);
            Assert.That(response.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));

            WebHelper.SetInternalServerError(response);
            Assert.That(response.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));

            WebHelper.SetStatus(response, 418);
            Assert.That(response.StatusCode, Is.EqualTo(418));
        }

        [Test]
        public void ResponseHelper_RedirectAsync_CoversStandardAndScriptPaths()
        {
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            ResponseHelper.RedirectAsync(context.Response, "/standard").GetAwaiter().GetResult();
            Assert.That(context.Response.StatusCode, Is.EqualTo(StatusCodes.Status302Found));
            Assert.That(context.Response.Headers["Location"].ToString(), Is.EqualTo("/standard"));

            context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            ResponseHelper.RedirectAsync(context.Response, "/popup", target: "_blank", windowFeatures: "width=500,height=500").GetAwaiter().GetResult();
            context.Response.Body.Position = 0;
            using var reader = new StreamReader(context.Response.Body, Encoding.UTF8, leaveOpen: true);
            string html = reader.ReadToEnd();

            Assert.That(context.Response.ContentType, Is.EqualTo("text/html"));
            Assert.That(html, Does.Contain("window.open"));
            Assert.That(html, Does.Contain("_blank"));
        }

        [Test]
        public void StreamHelper_CoversPositiveAndFailureBranches()
        {
            using var source = new MemoryStream(Encoding.UTF8.GetBytes("abcdef"));
            using var target = new MemoryStream();

            source.CopyTo(target, 2);
            Assert.That(Encoding.UTF8.GetString(target.ToArray()), Is.EqualTo("abcdef"));

            source.Position = 0;
            using var memoryCopy = source.CopyToMemory();
            Assert.That(memoryCopy.ReadToEnd(), Is.EqualTo("abcdef"));

            source.Position = 0;
            byte[] allBytes = source.ReadAllBytes();
            Assert.That(Encoding.UTF8.GetString(allBytes), Is.EqualTo("abcdef"));

            source.Position = 0;
            byte[]? fixedBytes = source.ReadFixedBuffersize(3);
            Assert.That(fixedBytes, Is.Not.Null);
            Assert.That(Encoding.UTF8.GetString(fixedBytes!), Is.EqualTo("abc"));

            source.Position = 0;
            Assert.That(source.SeekToEnd().Position, Is.EqualTo(source.Length));
            Assert.That(source.SeekToBegin().Position, Is.EqualTo(0));

            using var writeStream = new MemoryStream();
            writeStream.Write(Encoding.UTF8.GetBytes("xy"));
            Assert.That(Encoding.UTF8.GetString(writeStream.ToArray()), Is.EqualTo("xy"));

            // Keep explicit failure-path assertions: these protect guard-clauses that are easy to regress during refactors.
            Assert.Throws<ArgumentOutOfRangeException>(() => source.CopyTo(target, 0));
            Assert.Throws<ArgumentNullException>(() => StreamHelper.Write(writeStream, null!));
        }

        [Test]
        public void TransformationsHelper_CoversPublicMethodsAndEdgePaths()
        {
            using var command = new SqlCommand();
            command.UpsertParameter(new SqlParameter("@id", 1));
            command.UpsertParameter(new SqlParameter("@id", 2));
            Assert.That(command.Parameters.Count, Is.EqualTo(1));
            Assert.That(command.Parameters[0].Value, Is.EqualTo(2));

            var list = new List<SqlParameter> { new SqlParameter("@x", 1) };
            list.UpsertParameter(new SqlParameter("@x", 5));
            list.UpsertParameter(new SqlParameter("@y", 9));
            Assert.That(list.Count, Is.EqualTo(2));
            Assert.That(list.First(p => p.ParameterName == "@x").Value, Is.EqualTo(5));

            int hash1 = TransformationsHelper.ComputeHash("abc");
            int hash2 = TransformationsHelper.ComputeHash("abc");
            Assert.That(hash1, Is.EqualTo(hash2));

            var payload = new XmlPayload { Id = 7, Name = "test" };
            string xml = TransformationsHelper.ObjectToXml(payload, shouldPrettyPrint: false);
            string xml2 = payload.XmlSerialize();
            XmlPayload? back = xml2.XmlDeserialize<XmlPayload>();
            XmlPayload? failBack = "not xml".XmlDeserialize<XmlPayload>();

            Assert.That(xml, Does.Contain("XmlPayload"));
            Assert.That(xml2, Does.Contain("XmlPayload"));
            Assert.That(back, Is.Not.Null);
            Assert.That(back!.Id, Is.EqualTo(7));
            Assert.That(failBack, Is.Null);

            Assert.Throws<ArgumentNullException>(() => TransformationsHelper.ObjectToXml(null!, false));

            int convertedInt = TransformationsHelper.ConvertObjectTo("42", -1);
            Guid convertedGuid = TransformationsHelper.ConvertObjectTo("11111111-1111-1111-1111-111111111111", Guid.Empty);
            char convertedChar = TransformationsHelper.ConvertObjectTo("Zebra", 'x');

            bool tryOk = TransformationsHelper.TryConvertObjectTo("55", -1, out int tryInt);
            bool tryFail = TransformationsHelper.TryConvertObjectTo("oops", -1, out int tryFailInt);

            Assert.That(convertedInt, Is.EqualTo(42));
            Assert.That(convertedGuid, Is.EqualTo(Guid.Parse("11111111-1111-1111-1111-111111111111")));
            Assert.That(convertedChar, Is.EqualTo('Z'));
            Assert.That(tryOk, Is.True);
            Assert.That(tryInt, Is.EqualTo(55));
            Assert.That(tryFail, Is.False);
            Assert.That(tryFailInt, Is.EqualTo(-1));
        }

        [Test]
        public void Helper_CoversHashAndNullChecks()
        {
            object value = new object();
            object? nullValue = null;

            Assert.That(value.IsNotNull(), Is.True);
            Assert.That(nullValue.IsNull(), Is.True);

            int hash = Helper.ComputeHash("hello");
            int hashAgain = Helper.ComputeHash("hello");
            Assert.That(hash, Is.EqualTo(hashAgain));
        }

        public class XmlPayload
        {
            public int Id { get; set; }
            public string? Name { get; set; }
        }
    }
}
