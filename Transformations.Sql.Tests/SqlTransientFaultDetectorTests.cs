using System.Data;
using System.Linq;
using System.Reflection;
using Microsoft.Data.SqlClient;
using NUnit.Framework;
using Transformations.Dapper;

namespace Transformations.Dapper.Tests
{
    [TestFixture]
    public class SqlTransientFaultDetectorTests
    {
        private static SqlException CreateSqlException(int errorNumber)
        {
            // SqlException has no public constructor; build one via reflection.
            var errorCollection = (SqlErrorCollection)Activator.CreateInstance(
                typeof(SqlErrorCollection),
                BindingFlags.NonPublic | BindingFlags.Instance,
                null, null, null)!;

            var error = (SqlError)Activator.CreateInstance(
                typeof(SqlError),
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new object?[]
                {
                    errorNumber,      // infoNumber
                    (byte)0,          // errorState
                    (byte)0,          // errorClass
                    "server",         // server
                    "error message",  // errorMessage
                    "procedure",      // procedure
                    0,                // lineNumber
                    null              // exception
                },
                null)!;

            typeof(SqlErrorCollection)
                .GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Instance)!
                .Invoke(errorCollection, new object[] { error });

            var exception = (SqlException)Activator.CreateInstance(
                typeof(SqlException),
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new object?[]
                {
                    "Transient fault",  // message
                    errorCollection,    // errorCollection
                    null,               // innerException
                    Guid.Empty          // conId
                },
                null)!;

            return exception;
        }

        [TestCase(-2)]
        [TestCase(53)]
        [TestCase(121)]
        [TestCase(1205)]
        [TestCase(1221)]
        [TestCase(10053)]
        [TestCase(10054)]
        [TestCase(40197)]
        [TestCase(40501)]
        [TestCase(40613)]
        [TestCase(49918)]
        public void IsTransient_KnownTransientErrorNumber_ReturnsTrue(int errorNumber)
        {
            var ex = CreateSqlException(errorNumber);

            Assert.That(SqlTransientFaultDetector.IsTransient(ex), Is.True);
        }

        [TestCase(547)]
        [TestCase(2627)]
        [TestCase(8152)]
        [TestCase(0)]
        public void IsTransient_PermanentErrorNumber_ReturnsFalse(int errorNumber)
        {
            var ex = CreateSqlException(errorNumber);

            Assert.That(SqlTransientFaultDetector.IsTransient(ex), Is.False);
        }

        [Test]
        public void GetTransientErrorNumbers_IsTheDocumentedList()
        {
            // The README lists these codes. A change here must change the README and this list together.
            int[] documented =
            {
                -2, 53, 64, 121, 233, 921,
                1205, 1221, 1807,
                8628, 8645,
                10053, 10054, 10060,
                10928, 10929, 10936,
                14355, 17197,
                40143, 40197, 40501, 40540, 40613,
                41301, 41302, 41305, 41325, 41839,
                49918, 49919, 49920,
            };

            Assert.That(SqlTransientFaultDetector.GetTransientErrorNumbers().OrderBy(n => n), Is.EqualTo(documented.OrderBy(n => n)));
        }

        [Test]
        public void IsTransient_TimeoutException_ReturnsTrue()
        {
            Assert.That(SqlTransientFaultDetector.IsTransient(new TimeoutException()), Is.True);
        }

        [Test]
        public void IsTransient_GenericException_ReturnsFalse()
        {
            Assert.That(SqlTransientFaultDetector.IsTransient(new InvalidOperationException()), Is.False);
        }

        [Test]
        public void IsTransient_NullReferenceException_ReturnsFalse()
        {
            Assert.That(SqlTransientFaultDetector.IsTransient(new NullReferenceException()), Is.False);
        }

        [Test]
        public void GetTransientErrorNumbers_ReturnsNonEmptyCollection()
        {
            var numbers = SqlTransientFaultDetector.GetTransientErrorNumbers();

            Assert.That(numbers, Is.Not.Empty);
            Assert.That(numbers, Does.Contain(1205));
            Assert.That(numbers, Does.Contain(40613));
        }
    }
}
