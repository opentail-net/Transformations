namespace Transformations.Tests
{
    using System.Data;

    using Microsoft.Data.SqlClient;

    using NUnit.Framework;

    [TestFixture]
    public class FinalCoveragePushTests
    {
        [Test]
        public void ConverterOld_RemainingMethods_AreCovered()
        {
            bool fromString = "true".ToBool();
            ulong fromUlongString = "42".ToUlong();
            char yesNo = true.ToYesNoChar();

            Assert.That(fromString, Is.True);
            Assert.That(fromUlongString, Is.EqualTo(42UL));
            Assert.That(yesNo, Is.EqualTo('Y'));
        }

        [Test]
        public void AdditionalStringHelper_UrlPathEncode_IsCovered()
        {
            string encoded = "a b/c".UrlPathEncode();

            // Keep assertion encoding-style agnostic (runtime/framework may vary between %20 and + for spaces).
            Assert.That(encoded, Is.Not.EqualTo("a b/c"));
        }

        [Test]
        public void DataRowConverter_IsNumericValue_Overloads_AreCovered()
        {
            var table = new DataTable();
            table.Columns.Add("N", typeof(string));
            table.Columns.Add("T", typeof(string));
            table.Rows.Add("123", "abc");
            DataRow row = table.Rows[0];

            Assert.That(row.IsNumericValue("N"), Is.True);
            Assert.That(row.IsNumericValue(0), Is.True);
            Assert.That(row.IsNumericValue("T"), Is.False);
        }

        [Test]
        public void StringHelper_RemainingMethods_AreCovered()
        {
            string? copied = "abc".Copy();
            bool containsAnyCase = "AlphaBeta".ContainsAnyCase("beta", "x");
            string formatted = "Hi {0}".Format("Joe");
            string formatted2 = "Hi {0}".Format(out bool ok, "Joe");

            var kvp1 = "a=1;b=2".ToKeyValuePairList(new[] { ";" }, new[] { "=" });
            var kvp2 = "a=1|b=2".ToKeyValuePairList(new[] { "|" }, "=");
            var kvp3 = "a=1,b=2".ToKeyValuePairList(",", "=");

            var builder = "Server=(local);Database=Db;Trusted_Connection=True;".ParseConnectionString();
            bool parsed = "Server=(local);Database=Db;Trusted_Connection=True;".TryParseConnectionString(out var parsedBuilder);
            bool parseFail = "bad---connection".TryParseConnectionString(out var failBuilder);

            Assert.That(copied, Is.EqualTo("abc"));
            Assert.That(containsAnyCase, Is.True);
            Assert.That(formatted, Is.EqualTo("Hi Joe"));
            Assert.That(ok, Is.True);
            Assert.That(formatted2, Is.EqualTo("Hi Joe"));

            Assert.That(kvp1.Count, Is.EqualTo(2));
            Assert.That(kvp2.Count, Is.EqualTo(2));
            Assert.That(kvp3.Count, Is.EqualTo(2));

            Assert.That(builder.DataSource, Is.EqualTo("(local)"));
            Assert.That(parsed, Is.True);
            Assert.That(parsedBuilder.InitialCatalog, Is.EqualTo("Db"));
            Assert.That(parseFail, Is.False);
            Assert.That(failBuilder, Is.Not.Null);
        }

        [Test]
        public void Inspect_InCsv_And_SqlHelper_NewMethods_AreCovered()
        {
            Assert.That("b".InCsv("a,b,c"), Is.True);

            SqlParameter p = SqlHelper.NewSqlParameterAsVarchar("@x", "abcdef", 3);
            Assert.That(p.Value, Is.EqualTo("abc"));

            var list = new[] { new SqlParameter("@a", 1), new SqlParameter("@b", 2) };
            bool valid = list.ValidateParameters(out string message);

            Assert.That(valid, Is.True);
            Assert.That(message, Is.EqualTo("OK"));
        }
    }
}
