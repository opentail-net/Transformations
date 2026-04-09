namespace Transformations.Tests
{
    using System;

    using NUnit.Framework;

    [TestFixture]
    public class BasicTypeConverterBranchTests
    {
        [Test]
        public void Guid_ConvertTo_WithNonStringTarget_Throws()
        {
            Guid g = Guid.NewGuid();
            Guid? ng = g;

            Assert.Throws<ArgumentException>(() => g.ConvertTo<int>());
            Assert.Throws<ArgumentException>(() => ng.ConvertTo<int>());
        }

        [Test]
        public void DateTime_ConvertTo_Branches_ReturnExpectedDefaults()
        {
            DateTime now = new DateTime(2024, 1, 1);
            DateTime? nullable = null;

            // Convert.ChangeType(DateTime -> int) hits catch and returns default
            Assert.That(now.ConvertTo<int>(), Is.EqualTo(0));
            Assert.That(now.ConvertTo<int>(99), Is.EqualTo(99));
            Assert.That(nullable.ConvertTo<int>(), Is.EqualTo(0));
            Assert.That(nullable.ConvertTo<int>(77), Is.EqualTo(77));
        }

        [Test]
        public void String_ConvertTo_DefaultBranches_AreCovered()
        {
            Assert.That(((string?)null).ConvertTo<int>(), Is.EqualTo(0));
            Assert.That(string.Empty.ConvertTo<char>(), Is.EqualTo('\0'));
            Assert.That("not-a-guid".ConvertTo<Guid>(), Is.EqualTo(Guid.Empty));
            Assert.That("x".ConvertTo<char>(), Is.EqualTo('x'));
        }

        [Test]
        public void TryToChar_Branches_AreCovered()
        {
            bool ok1 = "AB".TryToChar(out char result1, defaultValue: 'Z', allowTruncating: false);
            bool ok2 = "AB".TryToChar(out char result2, defaultValue: 'Z', allowTruncating: true);
            bool ok3 = ((string)null!).TryToChar(out char result3, defaultValue: 'Q', allowTruncating: true);

            Assert.That(ok1, Is.False);
            Assert.That(result1, Is.EqualTo('Z'));
            Assert.That(ok2, Is.True);
            Assert.That(result2, Is.EqualTo('A'));
            Assert.That(ok3, Is.False);
            Assert.That(result3, Is.EqualTo('Q'));
        }

        [Test]
        public void TryToGuid_Branches_AreCovered()
        {
            bool shortInput = "abc".TryToGuid(out Guid g1, Guid.Parse("11111111-1111-1111-1111-111111111111"));
            bool longInput = ("11111111-1111-1111-1111-111111111111-extra").TryToGuid(out Guid g2);
            bool invalid36 = "zzzzzzzz-zzzz-zzzz-zzzz-zzzzzzzzzzzz".TryToGuid(out Guid g3, Guid.Parse("22222222-2222-2222-2222-222222222222"));

            Assert.That(shortInput, Is.False);
            Assert.That(g1, Is.EqualTo(Guid.Parse("11111111-1111-1111-1111-111111111111")));
            Assert.That(longInput, Is.True);
            Assert.That(g2, Is.EqualTo(Guid.Parse("11111111-1111-1111-1111-111111111111")));
            Assert.That(invalid36, Is.False);
            Assert.That(g3, Is.EqualTo(Guid.Parse("22222222-2222-2222-2222-222222222222")));
        }

        [Test]
        public void TryToDateTime_StringBranch_CoversSuccessAndFallback()
        {
            DateTime fallback = new DateTime(2000, 1, 1);

            bool ok = "15/02/2024".TryToDateTime(out DateTime d1, fallback);
            bool fail = "invalid".TryToDateTime(out DateTime d2, fallback);

            Assert.That(ok, Is.True);
            Assert.That(d1.Year, Is.EqualTo(2024));
            Assert.That(fail, Is.False);
            Assert.That(d2, Is.EqualTo(fallback));
        }

        [Test]
        public void TryToDateTime_NumericOverloads_CoverExcelAndTicksPaths()
        {
            // Excel branch (positive serial)
            Assert.That(39938.TryToDateTime(out DateTime ex1, dateValueType: BasicTypeConverter.DateValueType.Excel), Is.True);
            Assert.That(ex1.Year, Is.GreaterThan(1900));

            // Ticks branch with invalid ticks -> fallback/default
            DateTime fallback = new DateTime(1999, 1, 1);
            Assert.That((-1L).TryToDateTime(out DateTime t1, fallback, BasicTypeConverter.DateValueType.Ticks), Is.False);
            Assert.That(t1, Is.EqualTo(fallback));

            Assert.That(((short)-1).TryToDateTime(out DateTime t2, fallback, BasicTypeConverter.DateValueType.Ticks), Is.False);
            Assert.That(t2, Is.EqualTo(fallback));

            Assert.That(((ushort)1).TryToDateTime(out DateTime t3, fallback, BasicTypeConverter.DateValueType.Ticks), Is.True.Or.False);
            Assert.That(((ulong)1).TryToDateTime(out DateTime t4, fallback, BasicTypeConverter.DateValueType.Ticks), Is.True.Or.False);
            Assert.That(1.0d.TryToDateTime(out DateTime t5, fallback, BasicTypeConverter.DateValueType.Ticks), Is.True.Or.False);
            Assert.That(1.0f.TryToDateTime(out DateTime t6, fallback, BasicTypeConverter.DateValueType.Ticks), Is.True.Or.False);
        }

        [Test]
        public void DateTime_ToDouble_Wrapper_CoversExcelAndTicksModes()
        {
            DateTime input = new DateTime(2024, 6, 20, 10, 30, 0, DateTimeKind.Utc);

            double excel = input.ToDouble(dateValueType: BasicTypeConverter.DateValueType.Excel);
            double ticks = input.ToDouble(dateValueType: BasicTypeConverter.DateValueType.Ticks);

            Assert.That(excel, Is.Not.EqualTo(0d));
            Assert.That(ticks, Is.EqualTo(input.Ticks));
        }

        [Test]
        public void ConvertTo_FromNullableDouble_CoversDateTimeFallbackAndDefaultValuePaths()
        {
            double? nullValue = null;
            double? excelDate = 45200d;
            double? invalidDate = double.MaxValue;
            DateTime fallback = new DateTime(2001, 1, 1);

            Assert.That(nullValue.ConvertTo(123), Is.EqualTo(123));

            DateTime converted = excelDate.ConvertTo(fallback);
            DateTime fallbackResult = invalidDate.ConvertTo(fallback);

            Assert.That(converted.Year, Is.GreaterThan(1900));
            Assert.That(fallbackResult, Is.EqualTo(fallback));
        }

        [Test]
        public void ToEnum_CoversDefinedAndFallbackBranches()
        {
            DayOfWeek valid = "Friday".ToEnum(DayOfWeek.Sunday);
            DayOfWeek invalid = "NotARealDay".ToEnum(DayOfWeek.Sunday);

            Assert.That(valid, Is.EqualTo(DayOfWeek.Friday));
            Assert.That(invalid, Is.EqualTo(DayOfWeek.Sunday));
        }

        [Test]
        public void TryConvertTo_GuidAndDateTimeOverloads_CoverSuccessFailureAndNullBranches()
        {
            Guid guid = Guid.Parse("11111111-1111-1111-1111-111111111111");
            Guid? nullableGuid = null;
            DateTime now = new DateTime(2024, 1, 1);
            DateTime? nullableDate = null;

            bool guidOk = guid.TryConvertTo<int>(out string guidText);
            bool nullableGuidOk = nullableGuid.TryConvertTo<int>(out string? nullableGuidText);

            Assert.That(guidOk, Is.True);
            Assert.That(guidText, Is.EqualTo(guid.ToString().ToUpperInvariant()));
            Assert.That(nullableGuidOk, Is.True);
            Assert.That(nullableGuidText, Is.Null);

            bool dtFail = now.TryConvertTo<int>(out int intResult);
            bool nullableDtFail = nullableDate.TryConvertTo<int>(out int nullableIntResult);

            Assert.That(dtFail, Is.False);
            Assert.That(intResult, Is.EqualTo(0));
            Assert.That(nullableDtFail, Is.False);
            Assert.That(nullableIntResult, Is.EqualTo(0));
        }

        [Test]
        public void TryConvertTo_GuidGenericPlaceholder_ReturnsStringResult()
        {
            Guid guid = Guid.NewGuid();
            Guid? nullableGuid = guid;

            bool guidOk = guid.TryConvertTo<Guid>(out string guidText);
            bool nullableGuidOk = nullableGuid.TryConvertTo<Guid>(out string? nullableGuidText);

            Assert.That(guidOk, Is.True);
            Assert.That(guidText, Is.EqualTo(guid.ToString().ToUpperInvariant()));
            Assert.That(nullableGuidOk, Is.True);
            Assert.That(nullableGuidText, Is.EqualTo(guid.ToString().ToUpperInvariant()));
        }
    }
}
