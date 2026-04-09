namespace Transformations.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Xml.Linq;

    using Microsoft.Data.SqlClient;

    using NUnit.Framework;

    [TestFixture]
    public class SqlHelperCoverageTests
    {
        [Test]
        public void SqlParameter_Setters_CoverFluentHelpers()
        {
            var parameter = new SqlParameter();

            parameter
                .SetParameterName("@p")
                .SetSqlDbType(SqlDbType.VarChar)
                .SetSize(5)
                .SetDirection(ParameterDirection.InputOutput)
                .SetIsNullable(true)
                .SetSourceColumn("Name");

            Assert.That(parameter.ParameterName, Is.EqualTo("@p"));
            Assert.That(parameter.SqlDbType, Is.EqualTo(SqlDbType.VarChar));
            Assert.That(parameter.Size, Is.EqualTo(5));
            Assert.That(parameter.Direction, Is.EqualTo(ParameterDirection.InputOutput));
            Assert.That(parameter.IsNullable, Is.True);
            Assert.That(parameter.SourceColumn, Is.EqualTo("Name"));
        }

        [Test]
        public void SqlParameter_SetValue_CoversNullBitAndStringTruncationBranches()
        {
            var bitParam = new SqlParameter("@bit", SqlDbType.Bit);
            bitParam.SetValue((object)"yes");
            Assert.That(bitParam.Value, Is.EqualTo(1));

            bitParam.SetValue((object)"no");
            Assert.That(bitParam.Value, Is.EqualTo(0));

            bitParam.SetValue((object)"invalid");
            Assert.That(bitParam.Value, Is.EqualTo(DBNull.Value));

            var textParam = new SqlParameter("@txt", SqlDbType.VarChar) { Size = 3 };
            textParam.SetValue((object)"abcdef");
            Assert.That(textParam.Value, Is.EqualTo("abc"));

            textParam.SetValue((object?)null!);
            Assert.That(textParam.Value, Is.EqualTo(DBNull.Value));
        }

        [Test]
        public void SqlParameter_SetValue_StringMaxLength_AndBooleanShortcut()
        {
            var p = new SqlParameter("@x", SqlDbType.VarChar);
            p.SetValue("abcdef", 4);
            Assert.That(p.Value, Is.EqualTo("abcd"));

            p.SetValueFromBoolean(true);
            Assert.That(p.Value, Is.EqualTo(1));

            p.SetValueFromBoolean(false);
            Assert.That(p.Value, Is.EqualTo(0));
        }

        [Test]
        public void ToSqlParameter_CoversNullableAndXDocumentOverloads()
        {
            SqlParameter boolParam = ((bool?)true).ToSqlParameter("@b");
            SqlParameter nullBoolParam = ((bool?)null).ToSqlParameter("@nb");
            SqlParameter byteParam = ((byte?)7).ToSqlParameter("@by");
            SqlParameter sbyteParam = ((sbyte?)-2).ToSqlParameter("@sby");
            SqlParameter charParam = ((char?)'Z').ToSqlParameter("@ch");
            SqlParameter dtParam = DateTime.Today.ToSqlParameter("@dt");
            SqlParameter dtoParam = DateTimeOffset.Now.ToSqlParameter("@dto");
            SqlParameter xdocParam = new XDocument(new XElement("root", "x")).ToSqlParameter("@xml");
            SqlParameter nullXdocParam = ((XDocument?)null).ToSqlParameter("@xmln");

            Assert.That(boolParam.Value, Is.EqualTo(true));
            Assert.That(nullBoolParam.Value, Is.EqualTo(DBNull.Value));
            Assert.That(byteParam.Value, Is.EqualTo((byte)7));
            Assert.That(sbyteParam.Value, Is.EqualTo((short)-2));
            Assert.That(charParam.Value, Is.EqualTo('Z'));
            Assert.That(dtParam.ParameterName, Is.EqualTo("@dt"));
            Assert.That(dtoParam.ParameterName, Is.EqualTo("@dto"));
            Assert.That(xdocParam.Value.ToString(), Does.Contain("root"));
            Assert.That(nullXdocParam.Value, Is.EqualTo(DBNull.Value));
        }

        [Test]
        public void ToSqlParameterList_AndAddExtensions_WorkAsExpected()
        {
            var p1 = new SqlParameter("@a", 1);
            var p2 = new SqlParameter("@b", 2);
            ICollection<SqlParameter> list = p1.ToSqlParameterList(p2);

            list.Add(true, "@bool");
            list.Add(Guid.Parse("11111111-1111-1111-1111-111111111111"), "@guid");
            list.Add(5L, "@long");

            Assert.That(list.Count, Is.EqualTo(5));
            Assert.That(list, Has.Some.Matches<SqlParameter>(x => x.ParameterName == "@bool"));
            Assert.That(list, Has.Some.Matches<SqlParameter>(x => x.ParameterName == "@guid"));
            Assert.That(list, Has.Some.Matches<SqlParameter>(x => x.ParameterName == "@long"));
        }
    }
}
