using System.Data;
using System.Text;
using NUnit.Framework;

namespace Transformations.Tests
{
    [TestFixture]
    public class BugfixRegressionTests
    {
        private enum LongEnum : long { A = 1, B = 2 }
        private enum ByteEnum : byte { A = 1, B = 2 }
        private enum AliasEnum { A = 1, B = 1 }

        [Test]
        public void ToSbyte_LongBackedEnum_Converts() => Assert.That(LongEnum.B.ToSbyte(), Is.EqualTo((sbyte)2));

        [Test]
        public void ToInt_LongBackedEnum_OutOfRange_UsesDefault()
            => Assert.That(((LongEnum)(1L << 40)).ToInt(7), Is.EqualTo(7));

        [Test]
        public void ToEnum_Int_ByteBackedEnum_Works()
        {
            Assert.That(2.ToEnum<ByteEnum>(), Is.EqualTo(ByteEnum.B));
            Assert.That(99.TryToEnum<ByteEnum>(out _), Is.False);
            Assert.That(300.TryToEnum<ByteEnum>(out _), Is.False);
        }

        [Test]
        public void EnumToDictionary_AliasedValues_DoesNotThrow()
            => Assert.That(typeof(AliasEnum).EnumToDictionary()!.Count, Is.EqualTo(1));

        [Test]
        public void Csv_NegativeNumbers_AreNotPrefixed()
        {
            var dt = new DataTable();
            dt.Columns.Add("n", typeof(int));
            dt.Rows.Add(-5);
            Assert.That(dt.ToCsv(), Does.Contain("-5").And.Not.Contain("'-5"));
        }

        [Test]
        public void Csv_Text_FormulaStillNeutralised()
        {
            var dt = new DataTable();
            dt.Columns.Add("s", typeof(string));
            dt.Rows.Add("=1+1");
            Assert.That(dt.ToCsv(), Does.Contain("'=1+1"));
        }

        [Test]
        public void ToCsv_Enumerable_QuotesDelimiters()
        {
            Assert.That(new[] { "a,b", "c" }.ToCsv(), Is.EqualTo("\"a,b\",c"));
            Assert.That(new[] { -1, 2 }.ToCsv(';'), Is.EqualTo("-1;2"));
        }

        [Test]
        public void AddMonthsAndYearsSafely_ClampInsteadOfThrowing()
        {
            Assert.That(new DateTime(9999, 12, 1).AddMonthsSafely(1), Is.EqualTo(DateTime.MaxValue));
            Assert.That(new DateTime(1, 1, 15).AddMonthsSafely(-1), Is.EqualTo(DateTime.MinValue));
            Assert.That(new DateTime(9998, 12, 31).AddYearsSafely(1), Is.EqualTo(new DateTime(9999, 12, 31)));
            Assert.That(new DateTime(2024, 1, 31).AddMonthsSafely(1), Is.EqualTo(new DateTime(2024, 2, 29)));
        }

        [Test]
        public void TomorrowYesterday_UseSuppliedDate()
        {
            var d = new DateTime(2020, 1, 1, 13, 0, 0);
            Assert.That(d.Tomorrow(), Is.EqualTo(new DateTime(2020, 1, 2)));
            Assert.That(d.Yesterday(), Is.EqualTo(new DateTime(2019, 12, 31)));
        }

        [Test]
        public void ContainsIgnoreCase_NullElement_DoesNotThrow()
            => Assert.That(new string?[] { null, "x" }.ContainsIgnoreCase("X"), Is.True);

        [Test]
        public void ReadToEnd_LeavesStreamOpen()
        {
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes("hi"));
            Assert.That(ms.ReadToEnd(Encoding.UTF8), Is.EqualTo("hi"));
            Assert.That(ms.CanRead, Is.True);
        }
    }
}
