# Transformations Web smoke test

Runs the `Transformations.Web` helpers inside a real ASP.NET Core host, from a clean consumer. Each helper is called from an
endpoint, and each check drives that endpoint over HTTP and asserts what reached the wire: the status code, the headers, the
cookies and the body.

It is a release gate for you, not a CI job. Run it after changes to `Transformations.Web` or to the helpers it links from
`Transformations/` (`CookieHelper`, `SessionHelper`, `QueryStringHelper`, `WebHelper`, `ResponseHelper`, `ConfigurationHelper`).

## Run it

```powershell
pwsh ./tools/smoke/web/Run-WebSmoke.ps1
```

It packs Core and Web from the working tree, clears their NuGet cache entries, and runs the consumer on net8.0 and net10.0.
The host listens on `127.0.0.1` on a free port. Nothing needs installing beyond the .NET SDK, and the report is in `reports/`.

## What it checks

| Helper | Check |
|---|---|
| `QueryStringHelper` | `TryGetQuery<int>` reads an integer, and rejects a non-integer. `HasValue` and `GetAllQueryStrings` see the key. |
| `CookieHelper` | `Set` writes a cookie that `Get` and `Exists` read back, and `Delete` expires it. |
| `CookieHelper` | `Set` does not set `Secure` over plain HTTP. See below. |
| `SessionHelper` | `SetValue` persists across requests through the session cookie. |
| `WebHelper` | `Redirect(format, values)` writes 302 with the formatted `Location`. `SetFileNotFound` writes 404. `SetStatus` writes the code. |
| `ConfigurationHelper` | `GetSetting`, `GetValue<int>`, the fallback for a missing key, and `GetConnectionString`. |
| `SelectListExtensions` | `ToSelectList` returns one item per row, and marks the row whose value matches. |
| `ResponseHelper` | `RedirectAsync` writes a script that contains the URL and the window target. |

## Fixed: `CookieHelper.Set` followed a constant `Secure`

`CookieHelper.Set` used to set `Secure = true` whatever the request scheme. Browsers reject a Secure cookie set over plain HTTP
(except on `localhost`), so over HTTP the cookie was never stored. The helper now sets `Secure` from the request scheme, and
`Cookie:Secure` overrides it.

The checks now assert the fixed behaviour:

- `CookieIsNotSecureOverHttp` checks that `Set-Cookie` has no `Secure` attribute on an HTTP request.
- `CookieRoundTrip` reads the cookie back through the cookie container, with no explicit header. Before the fix this path
  dropped the cookie, so the check now covers the regression directly.

The HTTPS branch (`Secure` set) is covered by the unit tests in `Transformations.Tests/CookieHelperSecureTests.cs`, not by this
smoke test, which runs HTTP only.

The session cookie also round-trips through the cookie container over HTTP, so the session middleware's own cookie policy
does not set `Secure` here. Why it differs is not checked.

## Limits

- **HTTP only.** The host listens on plain HTTP. HTTPS, HSTS, and the `Secure` behaviour in a real browser are not exercised.
- **Shared host.** Each check gets a fresh `HttpClient` and cookie container, but all checks share one host and one in-memory
  session store.
- **`RedirectAsync` is checked by substring.** Its output is a script, so the check looks for the URL and the window target,
  not for the exact markup.
- **No authentication, no antiforgery, no MVC views.** The `SelectList` helper is checked as data, not as rendered HTML.

## Verification history

- `reports/20261026-225028-3.0.0.0-498da02.md`: PASS on net8.0 and net10.0, 12 of 12 checks each, clean tree
  (`dirty: False`), after the cookie fix. This is the report to cite.
- `reports/20261026-223613-3.0.0.0-378b9f3.md`: PASS on net8.0 and net10.0, 12 of 12 checks each, clean tree, before the
  cookie fix (it pins the old `Secure` behaviour, so it no longer describes the code)
  (`dirty: False`). An earlier run failed one check because the test read the JSON property `Text` where the wire format is
  `text`. The check was corrected; the library was not changed.
