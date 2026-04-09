namespace Transformations.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;

    using NUnit.Framework;

    /// <summary>
    /// Deep branch coverage for ConverterOld, CollectionConvertor, Inspect, and DataReaderHelper.
    /// Targets line/branch paths missed by method-name-only heuristic.
    /// </summary>
    [TestFixture]
    public class DeepBranchCoverageTests
    {
        #region ConverterOld — ToBool overloads (all numeric types, Y/N/default branches)

        [Test]
        public void ToBool_StringOverload_AllBranches()
        {
            // "Y" path
            Assert.That("Y".ToBool(), Is.True);
            // "N" path
            Assert.That("N".ToBool(), Is.False);
            // valid bool parse path
            Assert.That("true".ToBool(), Is.True);
            // invalid with null default → default(bool)
            Assert.That("garbage".ToBool(), Is.False);
            // invalid with explicit default
            Assert.That("garbage".ToBool(true), Is.True);
        }

        [Test]
        public void ToBool_CharOverload_AllBranches()
        {
            Assert.That('Y'.ToBool(), Is.True);
            Assert.That('1'.ToBool(), Is.True);
            Assert.That('N'.ToBool(), Is.False);
            Assert.That('0'.ToBool(), Is.False);
            // unknown char, null default
            Assert.That('X'.ToBool(), Is.False);
            // unknown char, explicit default
            Assert.That('X'.ToBool(true), Is.True);
        }

        [Test]
        public void ToBool_NumericOverloads_CoverAllTypesAndBranches()
        {
            // byte: 1=true, 0=false, other+null default, other+explicit default
            Assert.That(((byte)1).ToBool(), Is.True);
            Assert.That(((byte)0).ToBool(), Is.False);
            Assert.That(((byte)5).ToBool(), Is.False);
            Assert.That(((byte)5).ToBool(true), Is.True);

            // sbyte
            Assert.That(((sbyte)1).ToBool(), Is.True);
            Assert.That(((sbyte)0).ToBool(), Is.False);
            Assert.That(((sbyte)5).ToBool(), Is.False);
            Assert.That(((sbyte)5).ToBool(true), Is.True);

            // short
            Assert.That(((short)1).ToBool(), Is.True);
            Assert.That(((short)0).ToBool(), Is.False);
            Assert.That(((short)5).ToBool(), Is.False);
            Assert.That(((short)5).ToBool(true), Is.True);

            // ushort
            Assert.That(((ushort)1).ToBool(), Is.True);
            Assert.That(((ushort)0).ToBool(), Is.False);
            Assert.That(((ushort)5).ToBool(), Is.False);
            Assert.That(((ushort)5).ToBool(true), Is.True);

            // int
            Assert.That(1.ToBool(), Is.True);
            Assert.That(0.ToBool(), Is.False);
            Assert.That(5.ToBool(), Is.False);
            Assert.That(5.ToBool(true), Is.True);

            // uint
            Assert.That(1u.ToBool(), Is.True);
            Assert.That(0u.ToBool(), Is.False);
            Assert.That(5u.ToBool(), Is.False);
            Assert.That(5u.ToBool(true), Is.True);

            // long
            Assert.That(1L.ToBool(), Is.True);
            Assert.That(0L.ToBool(), Is.False);
            Assert.That(5L.ToBool(), Is.False);
            Assert.That(5L.ToBool(true), Is.True);

            // ulong
            Assert.That(1UL.ToBool(), Is.True);
            Assert.That(0UL.ToBool(), Is.False);
            Assert.That(5UL.ToBool(), Is.False);
            Assert.That(5UL.ToBool(true), Is.True);

            // float
            Assert.That(1f.ToBool(), Is.True);
            Assert.That(0f.ToBool(), Is.False);
            Assert.That(5f.ToBool(), Is.False);
            Assert.That(5f.ToBool(true), Is.True);

            // decimal
            Assert.That(1m.ToBool(), Is.True);
            Assert.That(0m.ToBool(), Is.False);
            Assert.That(5m.ToBool(), Is.False);
            Assert.That(5m.ToBool(true), Is.True);

            // double
            Assert.That(1d.ToBool(), Is.True);
            Assert.That(0d.ToBool(), Is.False);
            Assert.That(5d.ToBool(), Is.False);
            Assert.That(5d.ToBool(true), Is.True);
        }

        #endregion

        #region ConverterOld — ToChar overloads from numeric types

        [Test]
        public void ToChar_NumericOverloads_AreCovered()
        {
            Assert.That(((byte)65).ToChar(), Is.EqualTo('6'));
            Assert.That(((sbyte)7).ToChar(), Is.EqualTo('7'));
            Assert.That(((short)3).ToChar(), Is.EqualTo('3'));
            Assert.That(((ushort)4).ToChar(), Is.EqualTo('4'));
            Assert.That(5.ToChar(), Is.EqualTo('5'));
            Assert.That(6u.ToChar(), Is.EqualTo('6'));
            Assert.That(7L.ToChar(), Is.EqualTo('7'));
            Assert.That(8UL.ToChar(), Is.EqualTo('8'));
            Assert.That(9f.ToChar(), Is.EqualTo('9'));
            Assert.That(1m.ToChar(), Is.EqualTo('1'));
            Assert.That(2d.ToChar(), Is.EqualTo('2'));
        }

        [Test]
        public void ToChar_BoolOverload_AreCovered()
        {
            Assert.That(true.ToChar(), Is.EqualTo('1'));
            Assert.That(false.ToChar(), Is.EqualTo('0'));
        }

        [Test]
        public void ToChar_StringOverload_DefaultAndTruncateBranches()
        {
            // valid single char
            Assert.That("A".ToChar(), Is.EqualTo('A'));
            // truncate long string
            Assert.That("ABC".ToChar(allowTruncating: true), Is.EqualTo('A'));
            // no truncate, long string → default
            Assert.That("ABC".ToChar(defaultValue: 'X', allowTruncating: false), Is.EqualTo('X'));
            // empty string, null default → default(char)
            Assert.That("".ToChar(), Is.EqualTo('\0'));
            // null string, explicit default
            Assert.That(((string)null!).ToChar('Z'), Is.EqualTo('Z'));
        }

        #endregion

        #region ConverterOld — ToByte overloads (success + overflow + default branches)

        [Test]
        public void ToByte_StringAndNumericOverloads_AllBranches()
        {
            // string: success, fail+null default, fail+explicit default
            Assert.That("10".ToByte(), Is.EqualTo((byte)10));
            Assert.That("bad".ToByte(), Is.EqualTo((byte)0));
            Assert.That("bad".ToByte(99), Is.EqualTo((byte)99));

            // sbyte: valid, negative overflow+null default, negative overflow+explicit default
            Assert.That(((sbyte)5).ToByte(), Is.EqualTo((byte)5));
            Assert.That(((sbyte)-1).ToByte(), Is.EqualTo((byte)0));
            Assert.That(((sbyte)-1).ToByte(99), Is.EqualTo((byte)99));

            // short: valid, overflow
            Assert.That(((short)5).ToByte(), Is.EqualTo((byte)5));
            Assert.That(((short)999).ToByte(88), Is.EqualTo((byte)88));

            // ushort
            Assert.That(((ushort)5).ToByte(), Is.EqualTo((byte)5));
            Assert.That(((ushort)999).ToByte(77), Is.EqualTo((byte)77));

            // int
            Assert.That(5.ToByte(), Is.EqualTo((byte)5));
            Assert.That(999.ToByte(66), Is.EqualTo((byte)66));

            // uint
            Assert.That(5u.ToByte(), Is.EqualTo((byte)5));
            Assert.That(999u.ToByte(55), Is.EqualTo((byte)55));

            // long
            Assert.That(5L.ToByte(), Is.EqualTo((byte)5));
            Assert.That(999L.ToByte(44), Is.EqualTo((byte)44));

            // ulong
            Assert.That(5UL.ToByte(), Is.EqualTo((byte)5));
            Assert.That(999UL.ToByte(33), Is.EqualTo((byte)33));

            // float
            Assert.That(5f.ToByte(), Is.EqualTo((byte)5));
            Assert.That(999f.ToByte(22), Is.EqualTo((byte)22));

            // decimal
            Assert.That(5m.ToByte(), Is.EqualTo((byte)5));
            Assert.That(999m.ToByte(11), Is.EqualTo((byte)11));

            // double
            Assert.That(5d.ToByte(), Is.EqualTo((byte)5));
            Assert.That(999d.ToByte(9), Is.EqualTo((byte)9));
        }

        #endregion

        #region ConverterOld — ToInt overloads (overflow branches)

        [Test]
        public void ToInt_NumericOverloads_OverflowBranches()
        {
            // string
            Assert.That("42".ToInt(), Is.EqualTo(42));
            Assert.That("bad".ToInt(), Is.EqualTo(0));
            Assert.That("bad".ToInt(99), Is.EqualTo(99));

            // byte, sbyte, short, ushort (direct cast, always valid)
            Assert.That(((byte)5).ToInt(), Is.EqualTo(5));
            Assert.That(((sbyte)-5).ToInt(), Is.EqualTo(-5));
            Assert.That(((short)5).ToInt(), Is.EqualTo(5));
            Assert.That(((ushort)5).ToInt(), Is.EqualTo(5));

            // uint: valid + overflow
            Assert.That(5u.ToInt(), Is.EqualTo(5));
            Assert.That(uint.MaxValue.ToInt(), Is.EqualTo(0));
            Assert.That(uint.MaxValue.ToInt(99), Is.EqualTo(99));

            // long: valid + overflow
            Assert.That(5L.ToInt(), Is.EqualTo(5));
            Assert.That(long.MaxValue.ToInt(99), Is.EqualTo(99));

            // ulong: valid + overflow
            Assert.That(5UL.ToInt(), Is.EqualTo(5));
            Assert.That(ulong.MaxValue.ToInt(99), Is.EqualTo(99));

            // float: valid + overflow
            Assert.That(5f.ToInt(), Is.EqualTo(5));
            Assert.That(float.MaxValue.ToInt(99), Is.EqualTo(99));

            // decimal: valid + overflow
            Assert.That(5.9m.ToInt(), Is.EqualTo(5));
            Assert.That(decimal.MaxValue.ToInt(99), Is.EqualTo(99));

            // double: valid + overflow
            Assert.That(5.9d.ToInt(), Is.EqualTo(5));
            Assert.That(double.MaxValue.ToInt(99), Is.EqualTo(99));
        }

        #endregion

        #region ConverterOld — ToFloat overloads

        [Test]
        public void ToFloat_AllOverloads_AreCovered()
        {
            Assert.That("1.5".ToFloat(), Is.EqualTo(1.5f).Within(0.01f));
            Assert.That("bad".ToFloat(), Is.EqualTo(0f));
            Assert.That("bad".ToFloat(9.9f), Is.EqualTo(9.9f));

            Assert.That(1.ToFloat(), Is.EqualTo(1f));
            Assert.That(((short)2).ToFloat(), Is.EqualTo(2f));
            Assert.That(3L.ToFloat(), Is.EqualTo(3f));
            Assert.That(((byte)4).ToFloat(), Is.EqualTo(4f));
            Assert.That(5m.ToFloat(), Is.EqualTo(5f));
            Assert.That(6d.ToFloat(), Is.EqualTo(6f));
            Assert.That(((sbyte)7).ToFloat(), Is.EqualTo(7f));
            Assert.That(8UL.ToFloat(), Is.EqualTo(8f));
            Assert.That(((ushort)9).ToFloat(), Is.EqualTo(9f));
            Assert.That(10u.ToFloat(), Is.EqualTo(10f));
        }

        #endregion

        #region ConverterOld — remaining string parse methods (default branches)

        [Test]
        public void ConverterOld_StringParseMethods_DefaultBranches()
        {
            // ToGuid
            Assert.That("bad".ToGuid(), Is.EqualTo(Guid.Empty));
            Assert.That("bad".ToGuid(Guid.Parse("11111111-1111-1111-1111-111111111111")),
                Is.EqualTo(Guid.Parse("11111111-1111-1111-1111-111111111111")));
            Assert.That("11111111-1111-1111-1111-111111111111".ToGuid(),
                Is.EqualTo(Guid.Parse("11111111-1111-1111-1111-111111111111")));

            // ToDateTime
            Assert.That("bad".ToDateTime().Year, Is.EqualTo(1));
            Assert.That("bad".ToDateTime(new DateTime(2020, 1, 1)).Year, Is.EqualTo(2020));
            Assert.That("15/06/2024".ToDateTime().Year, Is.EqualTo(2024));

            // ToShort
            Assert.That("5".ToShort(), Is.EqualTo((short)5));
            Assert.That("bad".ToShort(), Is.EqualTo((short)0));
            Assert.That("bad".ToShort(99), Is.EqualTo((short)99));

            // ToLong
            Assert.That("5".ToLong(), Is.EqualTo(5L));
            Assert.That("bad".ToLong(), Is.EqualTo(0L));
            Assert.That("bad".ToLong(99), Is.EqualTo(99L));

            // ToDecimal
            Assert.That("1.5".ToDecimal(), Is.EqualTo(1.5m));
            Assert.That("bad".ToDecimal(), Is.EqualTo(0m));
            Assert.That("bad".ToDecimal(9.9m), Is.EqualTo(9.9m));

            // ToDouble
            Assert.That("1.5".ToDouble(), Is.EqualTo(1.5d));
            Assert.That("bad".ToDouble(), Is.EqualTo(0d));
            Assert.That("bad".ToDouble(9.9d), Is.EqualTo(9.9d));

            // ToSbyte
            Assert.That("5".ToSbyte(), Is.EqualTo((sbyte)5));
            Assert.That("bad".ToSbyte(), Is.EqualTo((sbyte)0));
            Assert.That("bad".ToSbyte(9), Is.EqualTo((sbyte)9));

            // ToUshort
            Assert.That("5".ToUshort(), Is.EqualTo((ushort)5));
            Assert.That("bad".ToUshort(), Is.EqualTo((ushort)0));
            Assert.That("bad".ToUshort(9), Is.EqualTo((ushort)9));

            // ToUint
            Assert.That("5".ToUint(), Is.EqualTo(5u));
            Assert.That("bad".ToUint(), Is.EqualTo(0u));
            Assert.That("bad".ToUint(9u), Is.EqualTo(9u));

            // ToUlong (already has partial coverage)
            Assert.That("bad".ToUlong(), Is.EqualTo(0UL));
            Assert.That("bad".ToUlong(9UL), Is.EqualTo(9UL));
        }

        #endregion

        #region CollectionConvertor — ConvertToArrayOfString branches

        [Test]
        public void ConvertToArrayOfString_AllBranches()
        {
            // null/empty input
            Assert.That("".ConvertToArrayOfString(), Is.Empty);
            // empty splitter
            Assert.That("abc".ConvertToArrayOfString(string.Empty), Is.EqualTo(new[] { "abc" }));
            // no splitter in value
            Assert.That("abc".ConvertToArrayOfString(","), Is.EqualTo(new[] { "abc" }));
            // splitter found
            Assert.That("a,b,c".ConvertToArrayOfString(","), Is.EqualTo(new[] { "a", "b", "c" }));
        }

        #endregion

        #region CollectionConvertor — ConvertToDataSet / ConvertToDataTable / ConvertToList

        [Test]
        public void CollectionConvertor_DataSetAndTableConversions_AllBranches()
        {
            // ConvertToDataSet with named table
            var table = new DataTable("MyTable");
            table.Columns.Add("Id", typeof(int));
            table.Rows.Add(1);
            DataSet ds = table.ConvertToDataSet();
            Assert.That(ds.DataSetName, Is.EqualTo("MyTable"));

            // ConvertToDataSet with explicit name
            var table2 = new DataTable();
            table2.Columns.Add("Id", typeof(int));
            table2.Rows.Add(1);
            DataSet ds2 = table2.ConvertToDataSet("CustomName");
            Assert.That(ds2.DataSetName, Is.EqualTo("CustomName"));

            // ConvertToDataTable from DataSet by index
            DataTable back = ds.ConvertToDataTable(0);
            Assert.That(back.Rows.Count, Is.EqualTo(1));

            // ConvertToDataTable from DataSet by name
            DataTable backByName = ds.ConvertToDataTable("MyTable");
            Assert.That(backByName.Rows.Count, Is.EqualTo(1));

            // ConvertToList from string
            List<int> intList = "1,2,3".ConvertToList<int>();
            Assert.That(intList, Is.EqualTo(new[] { 1, 2, 3 }));

            // ConvertToArray from string
            int[] intArr = "4,5".ConvertToArray<int>();
            Assert.That(intArr, Is.EqualTo(new[] { 4, 5 }));

            // ConvertToEnumerable from string
            var enumerable = "7,8".ConvertToEnumerable<int>();
            Assert.That(enumerable.Count(), Is.EqualTo(2));

            // ConvertToList from IEnumerable
            IList<int> listFromEnumerable = new[] { 1, 2, 3 }.ConvertToList();
            Assert.That(listFromEnumerable.Count, Is.EqualTo(3));

            // ConvertToList from Dictionary
            var dict = new Dictionary<int, int> { { 1, 10 }, { 2, 20 } };
            IList<int> fromDict = dict.ConvertToList();
            Assert.That(fromDict, Is.EqualTo(new[] { 10, 20 }));

            // ConvertToEnumerable cross-type
            var intSource = new List<int> { 1, 2, 3 };
            IEnumerable<long> longResult = intSource.ConvertToEnumerable<int, long>();
            Assert.That(longResult.Count(), Is.EqualTo(3));
        }

        #endregion

        #region CollectionConvertor — ConvertToBootstrapOptionList

        [Test]
        public void ConvertToBootstrapOptionList_AllBranches()
        {
            // DataTable overload
            var table = new DataTable();
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Url", typeof(string));
            table.Rows.Add("Google", "https://google.com");
            table.Rows.Add(DBNull.Value, "https://null.com");

            string html = table.ConvertToBootstrapOptionList("Name", "Url");
            Assert.That(html, Does.Contain("Google"));
            Assert.That(html, Does.Not.Contain("null.com"));

            // null guard
            Assert.That(((DataTable)null!).ConvertToBootstrapOptionList("A", "B"), Is.EqualTo(string.Empty));

            // Tuple overload
            var options = new List<(string Name, string Value)>
            {
                ("Alpha", "/alpha"),
                ("", "/empty"),
            };
            string tupleHtml = options.ToBootstrapOptionList();
            Assert.That(tupleHtml, Does.Contain("Alpha"));
            Assert.That(tupleHtml, Does.Not.Contain("empty"));

            // null tuple list
            Assert.That(((IEnumerable<(string, string)>)null!).ToBootstrapOptionList(), Is.EqualTo(string.Empty));
        }

        #endregion

        #region CollectionConvertor — ConvertToHashSet / ConvertToDataTable from ISet

        [Test]
        public void CollectionConvertor_HashSetAndTypedDataTable_AreCovered()
        {
            var table = new DataTable();
            table.Columns.Add("Id", typeof(int));
            table.Columns.Add("Name", typeof(string));
            table.Rows.Add(1, "Alice");
            table.Rows.Add(2, "Bob");

            HashSet<SimpleDto> set = table.ConvertToHashSet<SimpleDto>();
            Assert.That(set.Count, Is.EqualTo(2));
            Assert.That(set.Any(x => x.Name == "Alice"), Is.True);

            // ConvertToDataTable from ISet
            var sourceSet = new HashSet<SimpleDto> { new SimpleDto { Id = 10, Name = "Test" } };
            DataTable backTable = sourceSet.ConvertToDataTable<SimpleDto>();
            Assert.That(backTable.Rows.Count, Is.EqualTo(1));

            // ConvertToDataTable from ISet with group Guid
            DataTable withGuid = sourceSet.ConvertToDataTable("GroupId", Guid.NewGuid());
            Assert.That(withGuid.Columns.Contains("GroupId"), Is.True);
        }

        public class SimpleDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }

        #endregion

        #region Inspect — additional branch paths

        [Test]
        public void Inspect_Branches_AreCovered()
        {
            // IsDate with format
            Assert.That("15/02/2024".IsDate(), Is.True);
            Assert.That("bad".IsDate(), Is.False);
            Assert.That("02/15/2024".IsDate("en-US"), Is.True);

            // IsUpperCase / IsLowerCase
            Assert.That('A'.IsUpperCase(), Is.True);
            Assert.That('a'.IsUpperCase(), Is.False);
            Assert.That('a'.IsLowerCase(), Is.True);
            Assert.That('A'.IsLowerCase(), Is.False);

            // IsNumeric
            Assert.That("123".IsNumeric(), Is.True);
            Assert.That("12.5".IsNumeric(), Is.True);
            Assert.That("abc".IsNumeric(), Is.False);
            Assert.That("".IsNumeric(), Is.False);

            // WithDefault
            string? nullStr = null;
            Assert.That(nullStr.WithDefault("fallback"), Is.EqualTo("fallback"));
            Assert.That("value".WithDefault("fallback"), Is.EqualTo("value"));

            // In (string with separator)
            Assert.That("b".In("a,b,c", ','), Is.True);
            Assert.That("d".In("a,b,c", ','), Is.False);
            Assert.That("".In("a,b,c", ','), Is.False);
            Assert.That("x".In("x", ','), Is.True);

            // ContainsIgnoreCase
            var list = new List<string> { "Alpha", "Beta" };
            Assert.That(list.ContainsIgnoreCase("alpha"), Is.True);
            Assert.That(list.ContainsIgnoreCase("gamma"), Is.False);
        }

        #endregion

        #region DataReaderHelper — nullable accessor edge paths

        [Test]
        public void DataReaderHelper_NullableAccessors_NullColumnReturnsNull()
        {
            using var table = new DataTable();
            table.Columns.Add("NullInt", typeof(int));
            table.Columns.Add("NullGuid", typeof(Guid));
            table.Columns.Add("NullDate", typeof(DateTime));
            table.Columns.Add("NullDecimal", typeof(decimal));
            table.Columns.Add("NullBool", typeof(bool));
            table.Columns.Add("NullLong", typeof(long));
            table.Rows.Add(DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value);

            using var reader = table.CreateDataReader();
            Assert.That(reader.Read(), Is.True);

            Assert.That(reader.GetNullableInt("NullInt"), Is.Null);
            Assert.That(reader.GetNullableGuid("NullGuid"), Is.Null);
            Assert.That(reader.GetNullableDateTime("NullDate"), Is.Null);
            Assert.That(reader.GetNullableDateTimeOffset("NullDate"), Is.Null);
            Assert.That(reader.GetNullableDecimal("NullDecimal"), Is.Null);
            Assert.That(reader.GetNullableBoolean("NullBool"), Is.Null);
            Assert.That(reader.GetNullableLong("NullLong"), Is.Null);

            // Verify Int/Long return 0 for DBNull values
            Assert.That(reader.GetInt("NullInt"), Is.EqualTo(0));
            Assert.That(reader.GetLong("NullLong"), Is.EqualTo(0L));
        }

        [Test]
        public void DataReaderHelper_ReadAll_AndIndexOf_EdgeCases()
        {
            using var table = new DataTable();
            table.Columns.Add("X", typeof(int));

            // empty table → zero rows
            using var emptyReader = table.CreateDataReader();
            int count = emptyReader.ReadAll(_ => { });
            Assert.That(count, Is.EqualTo(0));

            // IndexOf null/empty
            Assert.That(((IDataRecord)null!).IndexOf("X"), Is.EqualTo(-1));
            using var reader2 = table.CreateDataReader();
            Assert.That(((IDataRecord)reader2).IndexOf(""), Is.EqualTo(-1));
        }

        [Test]
        public void DataReaderHelper_GetType_MissingAndInvalidPaths()
        {
            using var table = new DataTable();
            table.Columns.Add("TypeName", typeof(string));
            table.Columns.Add("BadType", typeof(string));
            table.Rows.Add(typeof(System.Text.StringBuilder).AssemblyQualifiedName, "Not.A.Real.Type.At.All");

            using var reader = table.CreateDataReader();
            Assert.That(reader.Read(), Is.True);

            // valid type
            Type validType = reader.GetType("TypeName", typeof(object));
            Assert.That(validType, Is.EqualTo(typeof(System.Text.StringBuilder)));

            // invalid type → fallback
            Type fallbackType = reader.GetType("BadType", typeof(object));
            Assert.That(fallbackType, Is.EqualTo(typeof(object)));

            // missing column
            Type missingType = reader.GetType("Missing", typeof(string));
            Assert.That(missingType, Is.EqualTo(typeof(string)));

            // GetTypeInstance
            object instance = reader.GetTypeInstance("TypeName");
            Assert.That(instance, Is.TypeOf<System.Text.StringBuilder>());
        }

        #endregion
    }
}
