namespace Transformations.Tests
{
    using System;
    using System.Collections.Generic;

    using NUnit.Framework;

    [TestFixture]
    public class DiagnosticExtensionsTests
    {
        [TearDown]
        public void ResetTraceGlobals()
        {
            DiagnosticExtensions.IsTraceEnabled = true;
            DiagnosticExtensions.TraceSink = message => System.Diagnostics.Debug.WriteLine(message);
        }

        [Test]
        public void Trace_IsChainable_AndWritesMessage()
        {
            var logs = new List<string>();
            DiagnosticExtensions.TraceSink = message => logs.Add(message);
            DiagnosticExtensions.IsTraceEnabled = true;

            int value = 42.Trace("answer");

            Assert.That(value, Is.EqualTo(42));
            Assert.That(logs.Count, Is.EqualTo(1));
            Assert.That(logs[0], Is.EqualTo("answer: 42"));
        }

        [Test]
        public void Trace_WhenDisabled_DoesNotWrite()
        {
            var logs = new List<string>();
            DiagnosticExtensions.TraceSink = message => logs.Add(message);
            DiagnosticExtensions.IsTraceEnabled = false;

            string text = "hello".Trace("greeting");

            Assert.That(text, Is.EqualTo("hello"));
            Assert.That(logs, Is.Empty);
        }

        [Test]
        public void Trace_NullValue_PrintsNullMarker()
        {
            var logs = new List<string>();
            DiagnosticExtensions.TraceSink = message => logs.Add(message);

            string? value = null;
            string? returned = value.Trace("payload");

            Assert.That(returned, Is.Null);
            Assert.That(logs.Count, Is.EqualTo(1));
            Assert.That(logs[0], Is.EqualTo("payload: [NULL]"));
        }

        [Test]
        public void Trace_EmptyLabel_UsesDefaultLabel()
        {
            var logs = new List<string>();
            DiagnosticExtensions.TraceSink = message => logs.Add(message);

            var returned = 7.Trace(string.Empty);

            Assert.That(returned, Is.EqualTo(7));
            Assert.That(logs[0], Is.EqualTo("Trace: 7"));
        }
    }
}
