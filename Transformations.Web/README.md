# Transformations.Web

Typed ASP.NET Core helpers for `IConfiguration`, session, cookies, query strings, HTTP responses, and MVC dropdowns — eliminating repetitive untyped boilerplate in controllers and middleware.

[![NuGet](https://img.shields.io/nuget/v/Transformations.Web.svg)](https://nuget.org/packages/Transformations.Web)
[![.NET 8 | 9 | 10](https://img.shields.io/badge/.NET-8%20%7C%209%20%7C%2010-blue)]()

## 📖 Overview

`Transformations.Web` provides typed, one-liner extensions for the ASP.NET Core surfaces you touch in every controller and middleware: `IConfiguration`, session, cookies, query strings, HTTP responses, and MVC dropdowns.

## 🚀 Why Transformations.Web?

ASP.NET Core's native APIs for configuration, session, and cookies all deal in untyped strings. Every controller ends up with the same parse-check-fallback boilerplate repeated across dozens of methods. These extensions collapse that pattern into a single typed call — `GetValue<T>(key, fallback)` — and bring the same discipline to query strings, cookie management, and `SelectList` construction.

## 📦 Install

```xml
<PackageReference Include="Transformations.Web" Version="2.0.2" />
```

---

## 💡 What's Included

### Typed IConfiguration Access

```csharp
// Read with explicit type and fallback
int limit = ConfigurationHelper.GetValue<int>(config, "Security:RateLimit", defaultValue: 100);

// Read as string with fallback
string dsn = ConfigurationHelper.GetSetting(config, "Sentry:Dsn", defaultValue: string.Empty);

// Connection string shortcut
string cs = ConfigurationHelper.GetConnectionString(config, "DefaultConnection");

// Existence check
bool hasKey = ConfigurationHelper.ContainsKey(config, "FeatureFlags:NewDashboard");
```

### Session State (Typed)

Session values are serialized to JSON and deserialized on read, so any serializable type works:

```csharp
// Store
SessionHelper.SetValue(httpContext, "cart", cartObject);

// Retrieve with fallback
var cart = SessionHelper.GetValue<ShoppingCart>(httpContext, "cart");

// Check existence
bool hasCart = SessionHelper.Exists(httpContext, "cart");

// Clear a single key or the whole session
SessionHelper.Remove(httpContext, "cart");
SessionHelper.Clear(httpContext);
```

### Query String Parsing

```csharp
// Typed read with fallback
int page = QueryStringHelper.TryGetQuery<int>(httpContext, "page", defaultValue: 1);

// Presence check
bool hasFilter = QueryStringHelper.HasValue(httpContext.Request.Query, "filter");

// Get all as dictionary
var all = QueryStringHelper.GetAllQueryStrings(httpContext);
```

### Cookie Management

```csharp
// ICookieHelper is DI-registered; inject it into your controller
string token = _cookies.Get(httpContext, "auth_token");
_cookies.Set(httpContext, "pref", value, new CookieOptions { HttpOnly = true });
_cookies.Delete(httpContext, "auth_token");
```

### HTTP Response Shortcuts

```csharp
WebHelper.Redirect(httpContext, "/dashboard");
WebHelper.SetFileNotFound(httpContext);
WebHelper.SetInternalServerError(httpContext);
WebHelper.SetStatus(httpContext, 429);

// Async redirect with optional window target
await ResponseHelper.RedirectAsync(httpContext, "/login", target: "_top");
```

### MVC SelectList Helpers

Convert a `DataTable` directly to a `List<SelectListItem>` for MVC/Razor dropdowns:

```csharp
// Basic — specify the text column and value column
var items = dataTable.ToSelectList(nameColumn: "RoleName", valueColumn: "RoleId");

// With pre-selected item (matches by value or by name, case-insensitive)
var items = dataTable.ToSelectList("RoleName", "RoleId",
    selectedValue: currentRoleId.ToString());
```

---

## 🛠 API Reference

| Class | Purpose | Key Members |
|-------|---------|-------------|
| `ConfigurationHelper` | `IConfiguration` access | `GetSetting`, `GetValue<T>`, `GetConnectionString`, `TryGetSetting`, `ContainsKey`, `GetManualSetting` |
| `QueryStringHelper` | Query string parsing | `TryGetQuery<T>`, `HasValue`, `GetAllQueryStrings`, `ParseQueryString` |
| `SessionHelper` | Typed session state | `GetValue<T>`, `SetValue<T>`, `Exists`, `GetAllStrings`, `Remove`, `Clear` |
| `CookieHelper` | Cookie management | `Get`, `Set`, `Delete` via `ICookieHelper` interface |
| `WebHelper` | HTTP response control | `Redirect`, `Reload`, `SetFileNotFound`, `SetInternalServerError`, `SetStatus` |
| `ResponseHelper` | Async response helpers | `RedirectAsync` with optional window target |
| `SelectListExtensions` | DataTable → dropdown | `ToSelectList(nameColumn, valueColumn, selectedValue?, selectedName?)` |

---

## 📦 Dependencies

- `Transformations.Core`
- `Microsoft.AspNetCore.App` (framework reference)
- `Microsoft.Extensions.Configuration`
- `Microsoft.Extensions.Configuration.Binder`

---

## License

[MIT](https://opensource.org/licenses/MIT) — Copyright © 2026 [opentail.net](https://opentail.net)
