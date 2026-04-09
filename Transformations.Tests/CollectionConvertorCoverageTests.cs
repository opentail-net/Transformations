namespace Transformations.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;

    using NUnit.Framework;

    [TestFixture]
    public class CollectionConvertorCoverageTests
    {
        [TestCase("a,b,c", ",", 3)]
        [TestCase("abc", ",", 1)]
        [TestCase("a|b|c", "|", 3)]
        [TestCase("abc", "", 1)]
        [TestCase("", ",", 0)]
        public void ConvertToArrayOfString_CoversSplitterBranches(string input, string splitter, int expectedCount)
        {
            string[] result = input.ConvertToArrayOfString(splitter);

            Assert.That(result.Length, Is.EqualTo(expectedCount));
        }

        [Test]
        public void ConvertToArrayOfString_NullInput_ReturnsEmpty()
        {
            string[] result = ((string)null!).ConvertToArrayOfString(",");

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void ConvertToList_Int_IgnoreEmptyTrue_SkipsEmptyItems()
        {
            List<int> result = "1,,2,3".ConvertToList<int>(",", ignoreNullElements: true, ignoreEmptyElements: true);

            Assert.That(result, Is.EqualTo(new[] { 1, 2, 3 }));
        }

        [Test]
        public void ConvertToList_Int_IgnoreEmptyFalse_ConvertsEmptyToDefault()
        {
            List<int> result = "1,,2".ConvertToList<int>(",", ignoreNullElements: true, ignoreEmptyElements: false);

            Assert.That(result, Is.EqualTo(new[] { 1, 0, 2 }));
        }

        [Test]
        public void ConvertToList_IntToDateTime_ImpossibleCast_ReturnsDefaultValues()
        {
            IEnumerable<int> source = new[] { 1, 2 };

            List<DateTime> result = source.ConvertToList<int, DateTime>();

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.All(d => d == default(DateTime)), Is.True);
        }

        [Test]
        public void ConvertToList_DoubleToInt_ConvertsAndRoundsPerConverterRules()
        {
            IEnumerable<double> source = new[] { 1.0, 2.9, 3.1 };

            List<int> result = source.ConvertToList<double, int>();

            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result[0], Is.EqualTo(1));
        }

        [Test]
        public void ConvertToDataSet_UsesDataTableNameWhenProvidedByTable()
        {
            DataTable table = new DataTable("Orders");

            DataSet dataSet = table.ConvertToDataSet();

            Assert.That(dataSet.DataSetName, Is.EqualTo("Orders"));
            Assert.That(dataSet.Tables.Count, Is.EqualTo(1));
        }

        [Test]
        public void ConvertToDataSet_UsesDefaultNameWhenNoTableName()
        {
            DataTable table = new DataTable();

            DataSet dataSet = table.ConvertToDataSet();

            Assert.That(dataSet.DataSetName, Is.EqualTo("DataSet"));
        }

        [Test]
        public void TryToDataTable_ByName_CoversSuccessAndFailureBranches()
        {
            DataSet dataSet = new DataSet();
            dataSet.Tables.Add(new DataTable("First"));
            dataSet.Tables.Add(new DataTable("Second"));

            bool success = dataSet.TryToDataTable(out DataTable byName, "Second");
            bool failure = dataSet.TryToDataTable(out DataTable missing, "Missing");

            Assert.That(success, Is.True);
            Assert.That(byName.TableName, Is.EqualTo("Second"));
            Assert.That(failure, Is.False);
            Assert.That(missing.Columns.Count, Is.EqualTo(0));
        }

        [TestCase(-1, false)]
        [TestCase(0, true)]
        [TestCase(1, true)]
        [TestCase(2, false)]
        public void TryToDataTable_ById_CoversBoundaryBranches(int id, bool expected)
        {
            DataSet dataSet = new DataSet();
            dataSet.Tables.Add(new DataTable("First"));
            dataSet.Tables.Add(new DataTable("Second"));

            bool success = dataSet.TryToDataTable(out DataTable result, id);

            Assert.That(success, Is.EqualTo(expected));
            if (expected)
            {
                Assert.That(result.TableName, Is.EqualTo(id == 0 ? "First" : "Second"));
            }
            else
            {
                Assert.That(result.Columns.Count, Is.EqualTo(0));
            }
        }

        [Test]
        public void TryToDataTable_EmptyDataSet_ReturnsFalseWithEmptyResult()
        {
            DataSet dataSet = new DataSet();

            bool successByName = dataSet.TryToDataTable(out DataTable byName, "Any");
            bool successById = dataSet.TryToDataTable(out DataTable byId, 0);

            Assert.That(successByName, Is.False);
            Assert.That(successById, Is.False);
            Assert.That(byName.Columns.Count, Is.EqualTo(0));
            Assert.That(byId.Columns.Count, Is.EqualTo(0));
        }

        [Test]
        public void ConvertToBootstrapOptionList_DataTable_SkipsEmptyRowsAndEncodes()
        {
            DataTable table = new DataTable();
            table.Columns.Add("Name");
            table.Columns.Add("Value");
            table.Rows.Add("Normal", "url");
            table.Rows.Add("", "skip");
            table.Rows.Add("<script>", "x&y");

            string html = table.ConvertToBootstrapOptionList("Name", "Value");

            Assert.That(html, Does.Contain("Normal"));
            Assert.That(html, Does.Not.Contain("skip"));
            Assert.That(html, Does.Contain("&lt;script&gt;"));
            Assert.That(html, Does.Contain("x&amp;y"));
        }

        [Test]
        public void ToBootstrapOptionList_TupleInput_SkipsWhitespaceRowsAndEncodes()
        {
            var options = new List<(string Name, string Value)>
            {
                ("One", "1"),
                (" ", "2"),
                ("<b>Safe</b>", "a&b")
            };

            string html = options.ToBootstrapOptionList();

            Assert.That(html, Does.Contain("One"));
            Assert.That(html, Does.Not.Contain("href='2'"));
            Assert.That(html, Does.Contain("&lt;b&gt;Safe&lt;/b&gt;"));
            Assert.That(html, Does.Contain("a&amp;b"));
        }
    }
}
