namespace Transformations.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;

    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;

    using NUnit.Framework;

    [TestFixture]
    public class CoverageEdgeCasesTests
    {
        [Test]
        public void CookieHelper_EndToEnd_AndNullGuards()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers["Cookie"] = "a=1; b=2";

            var accessor = new HttpContextAccessor { HttpContext = context };
            IConfiguration config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Cookie:Duration"] = "1",
                    ["Cookie:IsHttp"] = "true",
                })
                .Build();

            var helper = new CookieHelper(accessor, config);

            Assert.That(helper.Exists("a"), Is.True);
            Assert.That(helper.Get("a"), Is.EqualTo("1"));
            Assert.That(helper.Get(string.Empty), Is.Null);

            helper.Set("token", "abc");
            Assert.That(context.Response.Headers["Set-Cookie"].ToString(), Does.Contain("token=abc"));

            helper.Delete("a");
            Assert.That(context.Response.Headers["Set-Cookie"].ToString(), Does.Contain("a="));

            helper.DeleteAll();
            string cookieOps = context.Response.Headers["Set-Cookie"].ToString();
            Assert.That(cookieOps, Does.Contain("a="));
            Assert.That(cookieOps, Does.Contain("b="));
        }

        [Test]
        public void RangeSemanticsHelper_AndExtensionHelper_Overloads_AreCovered()
        {
            var list = new[] { 10, 20, 30 };

            Assert.That(2.BetweenInclusive(1, 2), Is.True);
            Assert.That(2.BetweenExclusive(1, 3), Is.True);
            Assert.That(2.ClampIndexToFirst(list), Is.EqualTo(2));
            Assert.That((-5).ClampIndexToFirst(list), Is.EqualTo(0));
            Assert.That(99.ClampIndexToLast(list), Is.EqualTo(list.Length - 1));
            Assert.That(99.ClampIndexToNext(list), Is.EqualTo(list.Length));

            Assert.That(1.Between(new List<int> { 1, 2, 3 }), Is.True);
            Assert.That(1.5f.Between(1f, 2f), Is.True);
            Assert.That(((sbyte)1).Between((sbyte)0, (sbyte)2), Is.True);
            Assert.That(((byte)1).Between((byte)0, (byte)2), Is.True);
            Assert.That(DateTime.Today.Between(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1)), Is.True);
            Assert.That(((ushort)1).Between((ushort)0, (ushort)2), Is.True);
            Assert.That(((short)1).Between((short)0, (short)2), Is.True);
            Assert.That(1.Between(0, 2), Is.True);
            Assert.That(((ulong)1).Between((ulong)0, (ulong)2), Is.True);
            Assert.That(((long)1).Between((long)0, (long)2), Is.True);
            Assert.That(1d.Between(0d, 2d), Is.True);
            Assert.That(1m.Between(0m, 2m), Is.True);
            Assert.That(1.BetweenExclusive(0, 2), Is.True);
            Assert.That(ExtensionHelper.In(2, 1, 2, 3), Is.True);
        }

        [Test]
        public void DataRowConverter_EdgeBranches_AreCovered()
        {
            var table = new DataTable();
            table.Columns.Add("Num", typeof(string));
            table.Columns.Add("Text", typeof(string));
            table.Rows.Add("123", "abc");
            DataRow row = table.Rows[0];

            Assert.That(table.HasRows(), Is.True);
            Assert.That(table.HasColumns(), Is.True);
            Assert.That(table.Columns[0].IsNumericType(), Is.False);
            Assert.That(row.IsNumericType("Missing"), Is.False);
            Assert.That(row.IsNumericType(99), Is.False);

            Assert.That(row.Exists("Num"), Is.True);
            Assert.That(row.Exists(1), Is.True);
            Assert.That(row.Exists("Missing"), Is.False);

            Assert.That(row.GetItemArrayAsList().Count, Is.EqualTo(2));
            Assert.That(row.GetItemArrayAsString(), Does.Contain("123"));

            Assert.That(row.GetStringValue("Num"), Is.EqualTo("123"));
            Assert.That(row.GetStringValue(0), Is.EqualTo("123"));
            Assert.That(row.GetStringValue("Missing"), Is.Null);

            Assert.That(row.GetValue<int>("Missing", -7), Is.EqualTo(-7));
            Assert.That(row.GetValue<int>(99, -8), Is.EqualTo(-8));
            Assert.That(row.TryGetValue<int>("Num", out int val), Is.True);
            Assert.That(val, Is.EqualTo(123));
        }

        [Test]
        public void SqlHelper_Validation_FailurePaths_AreCovered()
        {
            bool nullListResult = SqlHelper.ValidateParameters(null!, out string nullMsg);
            Assert.That(nullListResult, Is.False);
            Assert.That(nullMsg, Does.Contain("null"));

            var hasNull = new Microsoft.Data.SqlClient.SqlParameter[] { null! };
            bool nullEntryResult = hasNull.ValidateParameters(out string nullEntryMsg);
            Assert.That(nullEntryResult, Is.False);
            Assert.That(nullEntryMsg, Does.Contain("null"));

            var noName = new[] { new Microsoft.Data.SqlClient.SqlParameter { ParameterName = "" } };
            bool noNameResult = noName.ValidateParameters(out string noNameMsg);
            Assert.That(noNameResult, Is.False);
            Assert.That(noNameMsg, Does.Contain("name"));
        }
    }
}
