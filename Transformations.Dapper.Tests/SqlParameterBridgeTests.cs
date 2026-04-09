using System.Data;
using Microsoft.Data.SqlClient;
using NUnit.Framework;
using Transformations.Dapper;

namespace Transformations.Dapper.Tests
{
    [TestFixture]
    public class SqlParameterBridgeTests
    {
        [Test]
        public void ToSqlParameters_AnonymousObject_CreatesParametersForEachProperty()
        {
            var input = new { Id = 42, Name = "test", Active = true };

            IReadOnlyList<SqlParameter> result = SqlParameterBridge.ToSqlParameters(input);

            Assert.That(result, Has.Count.EqualTo(3));
            Assert.That(result[0].ParameterName, Is.EqualTo("@Id"));
            Assert.That(result[0].Value, Is.EqualTo(42));
            Assert.That(result[1].ParameterName, Is.EqualTo("@Name"));
            Assert.That(result[1].Value, Is.EqualTo("test"));
            Assert.That(result[2].ParameterName, Is.EqualTo("@Active"));
            Assert.That(result[2].Value, Is.EqualTo(true));
        }

        [Test]
        public void ToSqlParameters_NullPropertyValue_SetToDBNull()
        {
            var input = new { Value = (string?)null };

            IReadOnlyList<SqlParameter> result = SqlParameterBridge.ToSqlParameters(input);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Value, Is.EqualTo(DBNull.Value));
        }

        [Test]
        public void ToSqlParameters_NullObject_ThrowsArgumentNull()
        {
            Assert.Throws<ArgumentNullException>(() => SqlParameterBridge.ToSqlParameters(null!));
        }

        [Test]
        public void ToSqlParameters_WithTypeMappings_AppliesSqlDbType()
        {
            var input = new { Id = 42, Name = "test" };
            var mappings = new Dictionary<string, SqlDbType>
            {
                { "Name", SqlDbType.NVarChar }
            };

            IReadOnlyList<SqlParameter> result = SqlParameterBridge.ToSqlParameters(input, mappings);

            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[1].SqlDbType, Is.EqualTo(SqlDbType.NVarChar));
        }

        [Test]
        public void ToSqlParameters_EmptyObject_ReturnsEmptyList()
        {
            var input = new { };

            IReadOnlyList<SqlParameter> result = SqlParameterBridge.ToSqlParameters(input);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void ToSqlParameters_PocoObject_UsesPublicProperties()
        {
            var input = new TestPoco { Id = 7, Label = "abc" };

            IReadOnlyList<SqlParameter> result = SqlParameterBridge.ToSqlParameters(input);

            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0].ParameterName, Is.EqualTo("@Id"));
            Assert.That(result[0].Value, Is.EqualTo(7));
            Assert.That(result[1].ParameterName, Is.EqualTo("@Label"));
            Assert.That(result[1].Value, Is.EqualTo("abc"));
        }

        private class TestPoco
        {
            public int Id { get; set; }
            public string Label { get; set; } = string.Empty;
        }

        #region Generic (AOT-safe) overloads

        [Test]
        public void ToSqlParametersGeneric_PocoObject_CreatesParameters()
        {
            var input = new TestPoco { Id = 7, Label = "abc" };

            IReadOnlyList<SqlParameter> result = SqlParameterBridge.ToSqlParameters<TestPoco>(input);

            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0].ParameterName, Is.EqualTo("@Id"));
            Assert.That(result[0].Value, Is.EqualTo(7));
            Assert.That(result[1].ParameterName, Is.EqualTo("@Label"));
            Assert.That(result[1].Value, Is.EqualTo("abc"));
        }

        [Test]
        public void ToSqlParametersGeneric_NullObject_ThrowsArgumentNull()
        {
            Assert.Throws<ArgumentNullException>(() => SqlParameterBridge.ToSqlParameters<TestPoco>(null!));
        }

        [Test]
        public void ToSqlParametersGeneric_WithTypeMappings_AppliesSqlDbType()
        {
            var input = new TestPoco { Id = 42, Label = "test" };
            var mappings = new Dictionary<string, SqlDbType>
            {
                { "Label", SqlDbType.NVarChar }
            };

            IReadOnlyList<SqlParameter> result = SqlParameterBridge.ToSqlParameters<TestPoco>(input, mappings);

            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[1].SqlDbType, Is.EqualTo(SqlDbType.NVarChar));
        }

        #endregion Generic (AOT-safe) overloads
    }
}
