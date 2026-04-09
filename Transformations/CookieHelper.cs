using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Transformations
{
    /// <summary>
    /// The cookie helper interface.
    /// </summary>
    public interface ICookieHelper
    {
        /// <summary>
        /// Get
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>A string?</returns>
        string? Get(string key);

        /// <summary>
        /// Set
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="daysToExpiration">The days converts to expiration.</param>
        void Set(string key, string value, int? daysToExpiration = null);

        /// <summary>
        /// Delete
        /// </summary>
        /// <param name="key">The key.</param>
        void Delete(string key);

        /// <summary>
        /// Deletes the all.
        /// </summary>
        void DeleteAll();

        /// <summary>
        /// Exists
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>A bool</returns>
        bool Exists(string key);
    }

    /// <summary>
    /// The cookie helper.
    /// </summary>
    public class CookieHelper : ICookieHelper
    {
        /// <summary>
        /// The http context accessor.
        /// </summary>
        private readonly IHttpContextAccessor _httpContextAccessor;
        /// <summary>
        /// The default duration.
        /// </summary>
        private readonly int _defaultDuration;
        /// <summary>
        /// Is http only.
        /// </summary>
        private readonly bool _isHttpOnly;


        /// <summary>
        /// Initializes a new instance of the <see cref="CookieHelper"/> class.
        /// </summary>
        /// <param name="httpContextAccessor">The http context accessor.</param>
        /// <param name="configuration">The configuration.</param>
        public CookieHelper(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;

            // Option A: Use the indexer with a fallback (Low-Magic, Very Stable)
            var durationValue = configuration["Cookie:Duration"];
            _defaultDuration = int.TryParse(durationValue, out var d) ? d : 360;

            var isHttpValue = configuration["Cookie:IsHttp"];
            _isHttpOnly = bool.TryParse(isHttpValue, out var b) ? b : true;
        }

        ////public CookieHelper(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        ////{
        ////    _httpContextAccessor = httpContextAccessor;

        ////    // Modern Configuration: No more ConfigurationManager
        ////    _defaultDuration = configuration.GetValue<int>("Cookie:Duration", 360);
        ////    _isHttpOnly = configuration.GetValue<bool>("Cookie:IsHttp", true);
        ////}

        /// <summary>
        /// Gets the context.
        /// </summary>
        private HttpContext? Context => _httpContextAccessor.HttpContext;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>A bool</returns>
        public bool Exists(string key) => Context?.Request.Cookies.ContainsKey(key) ?? false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>A string?</returns>
        public string? Get(string key)
        {
            if (string.IsNullOrEmpty(key) || Context == null) return null;

            // In modern .NET, cookies are already decoded. 
            // Only use HtmlEncode if you are explicitly preventing XSS on display.
            return Context.Request.Cookies[key]?.Trim();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="daysToExpiration">The days converts to expiration.</param>
        public void Set(string key, string value, int? daysToExpiration = null)
        {
            if (string.IsNullOrEmpty(key) || Context == null) return;

            var options = new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(daysToExpiration ?? _defaultDuration),
                HttpOnly = _isHttpOnly,
                Secure = true, // Modern Best Practice
                SameSite = SameSiteMode.Lax
            };

            Context.Response.Cookies.Append(key, value, options);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key">The key.</param>
        public void Delete(string key)
        {
            if (Context == null) return;
            Context.Response.Cookies.Delete(key);
        }

        /// <summary>
        /// Deletes the all.
        /// </summary>
        public void DeleteAll()
        {
            if (Context == null) return;
            foreach (var cookie in Context.Request.Cookies.Keys)
            {
                Delete(cookie);
            }
        }
    }
}