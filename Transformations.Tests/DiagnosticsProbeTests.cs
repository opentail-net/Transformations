namespace Transformations.Tests
{
    using NUnit.Framework;

    [TestFixture]
    public class DiagnosticsProbeTests
    {
        [Test]
        public void GetProcessMetrics_ReturnsHighVisibilityValues_AndDoesNotThrow()
        {
            ProcessMetrics? metrics = null;

            Assert.DoesNotThrow(() => metrics = DiagnosticsProbe.GetProcessMetrics());
            Assert.That(metrics, Is.Not.Null);

            Assert.That(metrics!.CpuUsagePercent, Is.GreaterThanOrEqualTo(-1));
            Assert.That(metrics.CpuUsagePercent, Is.LessThanOrEqualTo(100));

            Assert.That(metrics.PrivateMemoryMb, Is.GreaterThanOrEqualTo(-1));
            Assert.That(metrics.ThreadCount, Is.GreaterThanOrEqualTo(-1));
            Assert.That(metrics.AvailableVramMb, Is.GreaterThanOrEqualTo(-1));
        }
    }
}
