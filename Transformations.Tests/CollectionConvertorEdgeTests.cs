namespace Transformations.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;

    using Microsoft.AspNetCore.Mvc.Rendering;

    using NUnit.Framework;

    [TestFixture]
    public class CollectionConvertorEdgeTests
    {
        private class BranchEntityBase
        {
            public string BaseName { get; set; } = string.Empty;
        }

        private class BranchEntity : BranchEntityBase
        {
            public int Id { get; set; }
            public int? OptionalValue { get; set; }
            public int BrokenInt { get; set; }
        }

        [Test]
        public void ConvertToList_GenericCrossType_CoversPrimitiveBranches()
        {
            Assert.That(new List<bool> { true, false }.ConvertToList<bool, bool>(), Is.EqualTo(new[] { true, false }));
            Assert.That(new List<byte> { 1, 2 }.ConvertToList<byte, byte>(), Is.EqualTo(new byte[] { 1, 2 }));
            Assert.That(new List<char> { 'a', 'b' }.ConvertToList<char, char>(), Is.EqualTo(new[] { 'a', 'b' }));
            Assert.That(new List<DateTime> { new DateTime(2024, 1, 1) }.ConvertToList<DateTime, DateTime>().Count, Is.EqualTo(1));
            Assert.That(new List<decimal> { 1.5m }.ConvertToList<decimal, decimal>(), Is.EqualTo(new[] { 1.5m }));
            Assert.That(new List<double> { 1.5d }.ConvertToList<double, double>(), Is.EqualTo(new[] { 1.5d }));
            Assert.That(new List<short> { 3 }.ConvertToList<short, short>(), Is.EqualTo(new short[] { 3 }));
            Assert.That(new List<ushort> { 4 }.ConvertToList<ushort, ushort>(), Is.EqualTo(new ushort[] { 4 }));
            Assert.That(new List<int> { 5 }.ConvertToList<int, int>(), Is.EqualTo(new[] { 5 }));
            Assert.That(new List<uint> { 6u }.ConvertToList<uint, uint>(), Is.EqualTo(new uint[] { 6u }));
            Assert.That(new List<long> { 7L }.ConvertToList<long, long>(), Is.EqualTo(new long[] { 7L }));
            Assert.That(new List<ulong> { 8UL }.ConvertToList<ulong, ulong>(), Is.EqualTo(new ulong[] { 8UL }));
            Assert.That(new List<float> { 9f }.ConvertToList<float, float>(), Is.EqualTo(new[] { 9f }));
        }

        [Test]
        public void ConvertToList_DataTable_CoversTypeCorrectionAndBaseClassMapping()
        {
            var table = new DataTable();
            table.Columns.Add("Id", typeof(int));
            table.Columns.Add("OptionalValue", typeof(int));
            table.Columns.Add("BrokenInt", typeof(string));
            table.Columns.Add("BaseName", typeof(string));
            table.Rows.Add(1, 2, "bad", "base-one");
            table.Rows.Add(2, DBNull.Value, "bad", "base-two");

            List<BranchEntity> corrected = table.ConvertToList<BranchEntity>(typeTestAndCorrection: true);
            List<BranchEntity> uncorrected = table.ConvertToList<BranchEntity>(typeTestAndCorrection: false);

            Assert.That(corrected.Count, Is.EqualTo(2));
            Assert.That(corrected[0].Id, Is.EqualTo(1));
            Assert.That(corrected[0].OptionalValue, Is.EqualTo(2));
            Assert.That(corrected[0].BrokenInt, Is.EqualTo(0));
            Assert.That(corrected[0].BaseName, Is.EqualTo("base-one"));
            Assert.That(corrected[1].OptionalValue, Is.Null);

            Assert.That(uncorrected.Count, Is.EqualTo(2));
            Assert.That(uncorrected[0].BaseName, Is.EqualTo("base-one"));
        }

        [Test]
        public void ToSelectList_CoversNullAndSingleSelectionBranches()
        {
            Assert.That(((DataTable?)null).ToSelectList("Name", "Value"), Is.Empty);

            var table = new DataTable();
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Value", typeof(string));
            table.Rows.Add("Alpha", "1");
            table.Rows.Add("ALPHA", "1");
            table.Rows.Add("Beta", "2");

            List<SelectListItem> byValue = table.ToSelectList("Name", "Value", selectedValue: "1");
            List<SelectListItem> byName = table.ToSelectList("Name", "Value", selectedName: "beta");

            Assert.That(byValue.Count(x => x.Selected), Is.EqualTo(1));
            Assert.That(byValue[0].Selected, Is.True);
            Assert.That(byValue[1].Selected, Is.False);
            Assert.That(byName.Single(x => x.Selected).Text, Is.EqualTo("Beta"));
        }

        [Test]
        public void ConvertToListOfString_And_ConvertToString_CoverEdgePaths()
        {
            Assert.That("a|b|c".ConvertToListOfString("|") , Is.EqualTo(new[] { "a", "b", "c" }));

            var table = new DataTable();
            table.Columns.Add("Id", typeof(int));
            table.Columns.Add("Name", typeof(string));
            table.Rows.Add(1, "A");
            table.Rows.Add(2, "B");

            List<string> withHeader = table.ConvertToListOfString(includeColumnNamesAsHeader: true);
            List<string> withoutHeader = table.ConvertToListOfString(includeColumnNamesAsHeader: false);
            List<string> nullTable = ((DataTable)null!).ConvertToListOfString();

            Assert.That(withHeader[0], Is.EqualTo("Id,Name"));
            Assert.That(withHeader.Count, Is.EqualTo(3));
            Assert.That(withoutHeader.Count, Is.EqualTo(2));
            Assert.That(nullTable, Is.Empty);

            string fromEnumerable = ((IEnumerable<string?>)new string?[] { "A", "", null, "B" }).ConvertToString("|");
            string fromObjects = new object?[] { "X", "", null, 7 }.ConvertToString("|");

            Assert.That(fromEnumerable, Is.EqualTo("A|B"));
            Assert.That(fromObjects, Is.EqualTo("X|7"));
        }
    }
}
