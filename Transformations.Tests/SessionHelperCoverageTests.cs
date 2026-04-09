namespace Transformations.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Http;

    using NUnit.Framework;

    [TestFixture]
    public class SessionHelperCoverageTests
    {
        [Test]
        public void SessionHelper_CoversAllPublicMethods()
        {
            var context = new DefaultHttpContext();
            context.Session = new FakeSession();

            SessionHelper.SetValue(context, "number", 42);
            SessionHelper.SetValue(context, "text", "abc");

            Assert.That(SessionHelper.Exists(context, "number"), Is.True);

            int number = SessionHelper.GetValue(context, "number", -1);
            string text = SessionHelper.GetValue(context, "text", string.Empty)!;
            string missing = SessionHelper.GetValue(context, "missing", "fallback")!;

            Assert.That(number, Is.EqualTo(42));
            Assert.That(text, Is.EqualTo("abc"));
            Assert.That(missing, Is.EqualTo("fallback"));

            Dictionary<string, string> all = SessionHelper.GetAllStrings(context);
            Assert.That(all.ContainsKey("number"), Is.True);
            Assert.That(all.ContainsKey("text"), Is.True);

            SessionHelper.Remove(context, "text");
            Assert.That(SessionHelper.Exists(context, "text"), Is.False);

            SessionHelper.Clear(context);
            Assert.That(SessionHelper.Exists(context, "number"), Is.False);
        }

        private sealed class FakeSession : ISession
        {
            // Intentional minimal in-memory ISession stub for deterministic unit tests (no ASP.NET pipeline dependency).
            private readonly Dictionary<string, byte[]> store = new Dictionary<string, byte[]>();

            public IEnumerable<string> Keys => this.store.Keys;

            public string Id { get; } = Guid.NewGuid().ToString();

            public bool IsAvailable => true;

            public void Clear() => this.store.Clear();

            public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

            public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

            public void Remove(string key) => this.store.Remove(key);

            public void Set(string key, byte[] value) => this.store[key] = value;

            public bool TryGetValue(string key, out byte[] value) => this.store.TryGetValue(key, out value!);
        }
    }

}
