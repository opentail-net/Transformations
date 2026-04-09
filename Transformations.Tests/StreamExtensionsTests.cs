namespace Transformations.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    using NUnit.Framework;

    [TestFixture]
    public class StreamExtensionsTests
    {
        [Test]
        public void ForEachLine_ProcessesLineByLine()
        {
            string content = "line1\nline2\nline3";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            var lines = new List<string>();

            stream.ForEachLine(line => lines.Add(line));

            Assert.That(lines, Is.EqualTo(new[] { "line1", "line2", "line3" }));
        }

        [Test]
        public void CopyToWithProgress_CopiesAndReportsProgress()
        {
            byte[] data = Enumerable.Range(0, 10000).Select(i => (byte)(i % 256)).ToArray();
            using var source = new MemoryStream(data);
            using var destination = new MemoryStream();
            var progressValues = new List<double>();

            source.CopyToWithProgress(destination, p => progressValues.Add(p), bufferSize: 512);

            Assert.That(destination.ToArray(), Is.EqualTo(data));
            Assert.That(progressValues.First(), Is.EqualTo(0));
            Assert.That(progressValues.Last(), Is.EqualTo(100));
            Assert.That(progressValues.Any(p => p > 0 && p < 100), Is.True);
        }

        [Test]
        public void CopyToWithProgress_InvalidBuffer_Throws()
        {
            using var source = new MemoryStream(new byte[] { 1, 2, 3 });
            using var destination = new MemoryStream();

            Assert.Throws<ArgumentOutOfRangeException>(() => source.CopyToWithProgress(destination, _ => { }, 0));
        }

        [Test]
        public void ForEachLine_InvalidArguments_Throw()
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes("x"));

            Assert.Throws<ArgumentNullException>(() => StreamExtensions.ForEachLine(null!, _ => { }));
            Assert.Throws<ArgumentNullException>(() => stream.ForEachLine(null!));
        }

        [Test]
        public void CopyToWithProgress_NullCallback_Throws()
        {
            using var source = new MemoryStream(new byte[] { 1, 2, 3 });
            using var destination = new MemoryStream();

            Assert.Throws<ArgumentNullException>(() => source.CopyToWithProgress(destination, null!));
        }
    }
}
