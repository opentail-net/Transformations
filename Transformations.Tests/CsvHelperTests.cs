namespace Transformations.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Data;

    using NUnit.Framework;

    [TestFixture]
    public class CsvHelperTests
    {
        #region ToCsv (IEnumerable)

        [Test]
        public void ToCsv_IntArray_ReturnsCommaSeparated()
        {
            //// Setup
            var values = new[] { 1, 2, 3, 4, 5 };
            string expected = "1,2,3,4,5";

            //// Act
            string actual = values.ToCsv();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ToCsv_StringList_ReturnsCommaSeparated()
        {
            //// Setup
            var values = new List<string> { "a", "b", "c" };
            string expected = "a,b,c";

            //// Act
            string actual = values.ToCsv();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ToCsv_EmptyCollection_ReturnsEmpty()
        {
            //// Setup
            var values = new List<int>();

            //// Act
            string actual = values.ToCsv();

            //// Assert
            Assert.That(actual, Is.EqualTo(string.Empty));
        }

        [Test]
        public void ToCsv_NullCollection_ReturnsEmpty()
        {
            //// Setup
            List<int> values = null!;

            //// Act
            string actual = values.ToCsv();

            //// Assert
            Assert.That(actual, Is.EqualTo(string.Empty));
        }

        [Test]
        public void ToCsv_CustomSeparator_ReturnsSemicolonSeparated()
        {
            //// Setup
            var values = new[] { 1, 2, 3 };
            string expected = "1;2;3";

            //// Act
            string actual = values.ToCsv(';');

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion ToCsv (IEnumerable)

        #region ToCsv (DataTable)

        [Test]
        public void ToCsv_DataTable_ReturnsHeaderAndRows()
        {
            //// Setup
            var table = new DataTable();
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Age", typeof(int));
            var row = table.NewRow();
            row["Name"] = "Alice";
            row["Age"] = 30;
            table.Rows.Add(row);

            //// Act
            string? actual = table.ToCsv();

            //// Assert
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual, Does.Contain("Name"));
            Assert.That(actual, Does.Contain("Alice"));
            Assert.That(actual, Does.Contain("30"));
        }

        [Test]
        public void ToCsv_DataTable_WithQualifier_WrapsValues()
        {
            //// Setup
            var table = new DataTable();
            table.Columns.Add("Name", typeof(string));
            var row = table.NewRow();
            row["Name"] = "Alice";
            table.Rows.Add(row);

            //// Act
            string? actual = table.ToCsv("\"");

            //// Assert
            Assert.That(actual, Does.Contain("\"Name\""));
            Assert.That(actual, Does.Contain("\"Alice\""));
        }

        [Test]
        public void ToCsv_NullDataTable_ReturnsNull()
        {
            //// Setup
            DataTable table = null!;

            //// Act
            string? actual = table.ToCsv();

            //// Assert
            Assert.That(actual, Is.Null);
        }

        [Test]
        public void ToCsv_EmptyDataTable_ReturnsHeaderOnly()
        {
            //// Setup
            var table = new DataTable();
            table.Columns.Add("Name", typeof(string));

            //// Act
            string? actual = table.ToCsv();

            //// Assert
            Assert.That(actual, Does.Contain("Name"));
        }

        #endregion ToCsv (DataTable)
    }
}
