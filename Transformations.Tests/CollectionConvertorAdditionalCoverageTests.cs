namespace Transformations.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Data;

    using NUnit.Framework;

    [TestFixture]
    public class CollectionConvertorAdditionalCoverageTests
    {
        [TestCase("1,2,3", 3)]
        [TestCase("", 0)]
        [TestCase("7", 1)]
        public void ConvertToArray_GenericInt_CoversWrapperMethod(string input, int expectedCount)
        {
            int[] result = input.ConvertToArray<int>();

            Assert.That(result.Length, Is.EqualTo(expectedCount));
        }

        [Test]
        public void ConvertToList_FromDictionary_ReturnsValuesOnly()
        {
            var dictionary = new Dictionary<int, int>
            {
                [1] = 10,
                [2] = 20,
                [3] = 30,
            };

            IList<int> result = dictionary.ConvertToList<int, int>();

            Assert.That(result, Is.EquivalentTo(new[] { 10, 20, 30 }));
        }

        [Test]
        public void ConvertToList_FromEnumerable_ReturnsEquivalentList()
        {
            IEnumerable<int> source = new[] { 5, 6, 7 };

            IList<int> result = source.ConvertToList<int>();

            Assert.That(result, Is.EqualTo(new[] { 5, 6, 7 }));
        }

        [Test]
        public void ConvertToDataTable_ByNameAndId_CoversBothWrapperOverloads()
        {
            DataSet dataSet = new DataSet();
            DataTable first = new DataTable("First");
            DataTable second = new DataTable("Second");
            dataSet.Tables.Add(first);
            dataSet.Tables.Add(second);

            DataTable byName = dataSet.ConvertToDataTable("Second");
            DataTable byId = dataSet.ConvertToDataTable(0);

            Assert.That(byName.TableName, Is.EqualTo("Second"));
            Assert.That(byId.TableName, Is.EqualTo("First"));
        }

        [Test]
        public void ToQueue_TimeSpan_EnqueuesBaseAndAdditionalValues()
        {
            Queue<TimeSpan> queue = TimeSpan.FromSeconds(1).ToQueue(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(3));

            Assert.That(queue.Count, Is.EqualTo(3));
            Assert.That(queue.Dequeue(), Is.EqualTo(TimeSpan.FromSeconds(1)));
            Assert.That(queue.Dequeue(), Is.EqualTo(TimeSpan.FromSeconds(2)));
            Assert.That(queue.Dequeue(), Is.EqualTo(TimeSpan.FromSeconds(3)));
        }

        [Test]
        public void ConvertToDataSet_WithExplicitName_UsesProvidedName()
        {
            DataTable table = new DataTable("IgnoredTableName");

            DataSet dataSet = table.ConvertToDataSet("ExplicitDataSet");

            Assert.That(dataSet.DataSetName, Is.EqualTo("ExplicitDataSet"));
        }
    }
}
