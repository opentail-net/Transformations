namespace Transformations.Tests
{
    using System;
    using System.Collections.Generic;

    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;

    using NUnit.Framework;

    [TestFixture]
    public class CookieHelperSecureTests
    {
        [TestCase("http", false)]
        [TestCase("https", true)]
        public void Set_FollowsRequestScheme_WhenNotConfigured(string scheme, bool expectedSecure)
        {
            string setCookie = SetCookieFor(scheme, configuredSecure: null);

            Assert.That(setCookie.Contains("secure", StringComparison.OrdinalIgnoreCase), Is.EqualTo(expectedSecure), setCookie);
        }

        [TestCase("http", "true", true)]
        [TestCase("https", "false", false)]
        public void Set_ConfiguredSecure_OverridesRequestScheme(string scheme, string configuredSecure, bool expectedSecure)
        {
            string setCookie = SetCookieFor(scheme, configuredSecure);

            Assert.That(setCookie.Contains("secure", StringComparison.OrdinalIgnoreCase), Is.EqualTo(expectedSecure), setCookie);
        }

        private static string SetCookieFor(string scheme, string? configuredSecure)
        {
            var context = new DefaultHttpContext();
            context.Request.Scheme = scheme;

            var values = new Dictionary<string, string?> { ["Cookie:Duration"] = "1" };
            if (configuredSecure is not null)
            {
                values["Cookie:Secure"] = configuredSecure;
            }

            IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection(values).Build();
            var helper = new CookieHelper(new HttpContextAccessor { HttpContext = context }, config);

            helper.Set("pref", "blue");
            return context.Response.Headers["Set-Cookie"].ToString();
        }
    }
}
