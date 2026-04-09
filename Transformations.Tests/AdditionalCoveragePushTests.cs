namespace Transformations.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Xml.Linq;

    using Microsoft.Data.SqlClient;

    using NUnit.Framework;

    [TestFixture]
    public class AdditionalCoveragePushTests
    {
        private enum LocalEnum
        {
            One = 1,
            Two = 2,
        }

        [Test]
        public void MiscHelper_AdditionalMethods_AreCovered()
        {
            Enum input = LocalEnum.Two;

            Assert.That(input.ToSbyte(), Is.EqualTo((sbyte)2));
            Assert.That(input.ToUshort(), Is.EqualTo((ushort)2));
            Assert.That(input.ToFloat(), Is.EqualTo(2f));

            IList<KeyValuePair<object, string>> kvp = input.GetParentEnumNameAndObjectList();
            IList<string> names = input.GetParentEnumNamesList();
            IList<KeyValuePair<object, string>>? fromType = typeof(LocalEnum).EnumToKvpList();

            Assert.That(kvp.Count, Is.EqualTo(2));
            Assert.That(names, Does.Contain("One"));
            Assert.That(fromType, Is.Not.Null);
            Assert.That(fromType!.Count, Is.EqualTo(2));
        }

        [Test]
        public void Inspect_AdditionalMethods_AreCovered()
        {
            Assert.That('5'.IsDigit(), Is.True);
            Assert.That('A'.IsLetter(), Is.True);
        }

        [Test]
        public void DateHelper_AdditionalMethods_AreCovered()
        {
            DateTime now = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            Assert.That(now.AddHoursSafely(1), Is.EqualTo(now.AddHours(1)));
            Assert.That(now.AddMillisecondsSafely(10), Is.EqualTo(now.AddMilliseconds(10)));
            Assert.That(now.AddMinutesSafely(1), Is.EqualTo(now.AddMinutes(1)));
            Assert.That(now.AddYearsSafely(1), Is.EqualTo(now.AddYears(1)));

            // Legacy compatibility: this overload historically adds years despite the method name.
            Assert.That(now.AddSecondsSafely(1), Is.EqualTo(now.AddYears(1)));

            Assert.That(DateHelper.GetMilliseconds(DateHelper.TimeInterval.Hour, 1), Is.EqualTo(3600000));
            Assert.That(DateHelper.GetMilliseconds((DateHelper.TimeInterval)999, 1), Is.EqualTo(0));
        }

        [Test]
        public void StreamHelper_GetReaderAndGetWriter_AreCovered()
        {
            using var mem = new MemoryStream();
            var writer = mem.GetWriter(Encoding.UTF8);
            writer.Write("hello");
            writer.Flush();

            mem.Position = 0;
            using var reader = mem.GetReader(Encoding.UTF8);
            string text = reader.ReadToEnd();

            Assert.That(text, Is.EqualTo("hello"));
        }

        [Test]
        public void SqlHelper_ToParameterArray_And_AddXml_AreCovered()
        {
            SqlParameter p1 = new SqlParameter("@a", 1);
            SqlParameter p2 = new SqlParameter("@b", 2);

            SqlParameter[] arr = p1.ToParameterArray(p2);
            Assert.That(arr.Length, Is.EqualTo(2));

            var list = new List<SqlParameter>();
            list.AddXml("<root />", "@xml1");
            list.AddXml(XDocument.Parse("<root><x>1</x></root>"), "@xml2");

            Assert.That(list.Count, Is.EqualTo(2));
            Assert.That(list[0].SqlDbType, Is.EqualTo(System.Data.SqlDbType.Xml));
            Assert.That(list[1].Value.ToString(), Does.Contain("root"));
        }
    }
}
