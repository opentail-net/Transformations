namespace Transformations.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Data;

    using NUnit.Framework;

    [TestFixture]
    public class DataRowConverterTests
    {
        #region Helpers

        private static DataTable BuildTable()
        {
            var table = new DataTable();
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Age", typeof(int));
            table.Columns.Add("Score", typeof(double));
            return table;
        }

        private static DataRow BuildRow(string name, object age, object score)
        {
            var table = BuildTable();
            var row = table.NewRow();
            row["Name"] = name;
            row["Age"] = age;
            row["Score"] = score;
            table.Rows.Add(row);
            return row;
        }

        #endregion Helpers

        #region HasRows

        [Test]
        public void HasRows_TableWithRows_ReturnsTrue()
        {
            //// Setup
            var table = BuildTable();
            table.Rows.Add(table.NewRow());
            const bool expected = true;

            //// Act
            bool actual = table.HasRows();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void HasRows_EmptyTable_ReturnsFalse()
        {
            //// Setup
            var table = BuildTable();
            const bool expected = false;

            //// Act
            bool actual = table.HasRows();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void HasRows_NullTable_ReturnsFalse()
        {
            //// Setup
            DataTable table = null!;
            const bool expected = false;

            //// Act
            bool actual = table.HasRows();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion HasRows

        #region HasColumns

        [Test]
        public void HasColumns_TableWithColumns_ReturnsTrue()
        {
            //// Setup
            var table = BuildTable();
            const bool expected = true;

            //// Act
            bool actual = table.HasColumns();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void HasColumns_EmptyTable_ReturnsFalse()
        {
            //// Setup
            var table = new DataTable();
            const bool expected = false;

            //// Act
            bool actual = table.HasColumns();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void HasColumns_NullTable_ReturnsFalse()
        {
            //// Setup
            DataTable table = null!;
            const bool expected = false;

            //// Act
            bool actual = table.HasColumns();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion HasColumns

        #region IsNumericType

        [Test]
        public void IsNumericType_IntColumn_ReturnsTrue()
        {
            //// Setup
            var row = BuildRow("Alice", 30, 99.5);
            const bool expected = true;

            //// Act
            bool actual = row.IsNumericType("Age");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void IsNumericType_StringColumn_ReturnsFalse()
        {
            //// Setup
            var row = BuildRow("Alice", 30, 99.5);
            const bool expected = false;

            //// Act
            bool actual = row.IsNumericType("Name");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void IsNumericType_ByIndex_NumericColumn_ReturnsTrue()
        {
            //// Setup
            var row = BuildRow("Alice", 30, 99.5);
            const bool expected = true;

            //// Act
            bool actual = row.IsNumericType(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void IsNumericType_ByIndex_StringColumn_ReturnsFalse()
        {
            //// Setup
            var row = BuildRow("Alice", 30, 99.5);
            const bool expected = false;

            //// Act
            bool actual = row.IsNumericType(0);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void IsNumericType_InvalidColumnName_ReturnsFalse()
        {
            //// Setup
            var row = BuildRow("Alice", 30, 99.5);
            const bool expected = false;

            //// Act
            bool actual = row.IsNumericType("NonExistent");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void IsNumericType_NegativeIndex_ReturnsFalse()
        {
            //// Setup
            var row = BuildRow("Alice", 30, 99.5);
            const bool expected = false;

            //// Act
            bool actual = row.IsNumericType(-1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion IsNumericType

        #region Exists

        [Test]
        public void Exists_ExistingColumnName_ReturnsTrue()
        {
            //// Setup
            var row = BuildRow("Alice", 30, 99.5);
            const bool expected = true;

            //// Act
            bool actual = row.Exists("Name");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Exists_NonExistingColumnName_ReturnsFalse()
        {
            //// Setup
            var row = BuildRow("Alice", 30, 99.5);
            const bool expected = false;

            //// Act
            bool actual = row.Exists("NonExistent");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Exists_ValidColumnIndex_ReturnsTrue()
        {
            //// Setup
            var row = BuildRow("Alice", 30, 99.5);
            const bool expected = true;

            //// Act
            bool actual = row.Exists(0);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Exists_NegativeColumnIndex_ReturnsFalse()
        {
            //// Setup
            var row = BuildRow("Alice", 30, 99.5);
            const bool expected = false;

            //// Act
            bool actual = row.Exists(-1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Exists_OutOfRangeColumnIndex_ReturnsFalse()
        {
            //// Setup
            var row = BuildRow("Alice", 30, 99.5);
            const bool expected = false;

            //// Act
            bool actual = row.Exists(100);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion Exists

        #region GetStringValue

        [Test]
        public void GetStringValue_ByName_ValidColumn_ReturnsValue()
        {
            //// Setup
            var row = BuildRow("Alice", 30, 99.5);
            string? expected = "Alice";

            //// Act
            string? actual = row.GetStringValue("Name");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void GetStringValue_ByName_NonExistingColumn_ReturnsNull()
        {
            //// Setup
            var row = BuildRow("Alice", 30, 99.5);

            //// Act
            string? actual = row.GetStringValue("NonExistent");

            //// Assert
            Assert.That(actual, Is.Null);
        }

        [Test]
        public void GetStringValue_ByName_DBNullValue_ReturnsNull()
        {
            //// Setup
            var table = BuildTable();
            var row = table.NewRow();
            row["Name"] = DBNull.Value;
            row["Age"] = 30;
            row["Score"] = 99.5;
            table.Rows.Add(row);
            string? expected = null;

            //// Act
            string? actual = row.GetStringValue("Name");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void GetStringValue_ByIndex_ValidColumn_ReturnsValue()
        {
            //// Setup
            var row = BuildRow("Alice", 30, 99.5);
            string? expected = "Alice";

            //// Act
            string? actual = row.GetStringValue(0);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void GetStringValue_ByIndex_OutOfRange_ReturnsNull()
        {
            //// Setup
            var row = BuildRow("Alice", 30, 99.5);

            //// Act
            string? actual = row.GetStringValue(100);

            //// Assert
            Assert.That(actual, Is.Null);
        }

        #endregion GetStringValue

        #region GetValue

        [Test]
        public void GetValue_ByName_IntColumn_ReturnsValue()
        {
            //// Setup
            var row = BuildRow("Alice", 30, 99.5);
            int expected = 30;

            //// Act
            int actual = row.GetValue<int>("Age");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void GetValue_ByName_NonExistingColumn_ReturnsDefault()
        {
            //// Setup
            var row = BuildRow("Alice", 30, 99.5);
            int expected = 0;

            //// Act
            int actual = row.GetValue<int>("NonExistent");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void GetValue_ByIndex_IntColumn_ReturnsValue()
        {
            //// Setup
            var row = BuildRow("Alice", 30, 99.5);
            int expected = 30;

            //// Act
            int actual = row.GetValue<int>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion GetValue

        #region TryGetValue

        [Test]
        public void TryGetValue_ByName_ValidColumn_ReturnsTrueAndValue()
        {
            //// Setup
            var row = BuildRow("Alice", 30, 99.5);
            const bool expectedOutcome = true;
            int expectedValue = 30;

            //// Act
            bool actual = row.TryGetValue<int>("Age", out int result);

            //// Assert
            Assert.That(actual, Is.EqualTo(expectedOutcome));
            Assert.That(result, Is.EqualTo(expectedValue));
        }

        [Test]
        public void TryGetValue_ByName_NonExistingColumn_ReturnsFalseAndDefault()
        {
            //// Setup
            var row = BuildRow("Alice", 30, 99.5);
            const bool expectedOutcome = false;
            int expectedValue = 0;

            //// Act
            bool actual = row.TryGetValue<int>("NonExistent", out int result);

            //// Assert
            Assert.That(actual, Is.EqualTo(expectedOutcome));
            Assert.That(result, Is.EqualTo(expectedValue));
        }

        [Test]
        public void TryGetValue_ByIndex_ValidColumn_ReturnsTrueAndValue()
        {
            //// Setup
            var row = BuildRow("Alice", 30, 99.5);
            const bool expectedOutcome = true;
            int expectedValue = 30;

            //// Act
            bool actual = row.TryGetValue<int>(1, out int result);

            //// Assert
            Assert.That(actual, Is.EqualTo(expectedOutcome));
            Assert.That(result, Is.EqualTo(expectedValue));
        }

        #endregion TryGetValue

        #region GetItemArrayAsList / GetItemArrayAsString

        [Test]
        public void GetItemArrayAsList_ValidRow_ReturnsAllValues()
        {
            //// Setup
            var row = BuildRow("Alice", 30, 99.5);
            int expected = 3;

            //// Act
            IList<object?> actual = row.GetItemArrayAsList();

            //// Assert
            Assert.That(actual.Count, Is.EqualTo(expected));
            Assert.That(actual[0], Is.EqualTo("Alice"));
        }

        [Test]
        public void GetItemArrayAsString_ValidRow_ReturnsCommaSeparated()
        {
            //// Setup
            var row = BuildRow("Alice", 30, 99.5);
            string expected = "Alice,30,99.5";

            //// Act
            string actual = row.GetItemArrayAsString();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion GetItemArrayAsList / GetItemArrayAsString
    }
}
