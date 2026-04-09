namespace Transformations.Tests
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;

    using NUnit.Framework;

    [TestFixture]
    public class CollectionConvertorApiSurfaceTests
    {
        [TestCase("a,b,c", ",", 3)]
        [TestCase("abc", ",", 1)]
        [TestCase("", ",", 0)]
        public void ConvertToListOfString_FromString_CoversWrapper(string input, string splitter, int expectedCount)
        {
            List<string> result = input.ConvertToListOfString(splitter);

            Assert.That(result.Count, Is.EqualTo(expectedCount));
        }

        [Test]
        public void ConvertToListOfString_FromDataTable_WithHeaderIncludesColumnNames()
        {
            DataTable table = new DataTable();
            table.Columns.Add("Name");
            table.Columns.Add("Value");
            table.Rows.Add("A", "1");

            List<string> lines = table.ConvertToListOfString(includeColumnNamesAsHeader: true);

            Assert.That(lines.Count, Is.EqualTo(2));
            Assert.That(lines[0], Is.EqualTo("Name,Value"));
            Assert.That(lines[1], Is.EqualTo("A,1"));
        }

        [Test]
        public void ConvertToString_IEnumerable_SkipsNullAndEmptyItems()
        {
            IEnumerable<string?> items = new string?[] { "A", null, string.Empty, "B" };

            string result = items.ConvertToString("|");

            Assert.That(result, Is.EqualTo("A|B"));
        }

        [Test]
        public void ConvertToString_ObjectArray_SkipsNullAndEmptyItems()
        {
            object?[] items = { "A", null, string.Empty, 2 };

            string result = items.ConvertToString(";");

            Assert.That(result, Is.EqualTo("A;2"));
        }

        [Test]
        public void ConvertToString_IList_And_IDictionary_CoversOverloads()
        {
            IList list = new ArrayList { "A", null, string.Empty, "B" };
            IDictionary dict = new Hashtable { ["K1"] = "V1", ["K2"] = "V2" };

            string? listResult = list.ConvertToString(",");
            string dictResult = dict.ConvertToString("|");

            Assert.That(listResult, Is.EqualTo("A,B"));
            Assert.That(dictResult, Does.Contain("K1"));
            Assert.That(dictResult, Does.Contain("K2"));
        }

        [Test]
        public void ToArray_Wrapper_CoversPrimitiveOverloads()
        {
            int[] ints = 1.ToArray(2, 3);
            bool[] bools = true.ToArray(false, true);
            char[] chars = 'a'.ToArray('b');
            double[] doubles = 1.1d.ToArray(2.2d);

            Assert.That(ints, Is.EqualTo(new[] { 1, 2, 3 }));
            Assert.That(bools, Is.EqualTo(new[] { true, false, true }));
            Assert.That(chars, Is.EqualTo(new[] { 'a', 'b' }));
            Assert.That(doubles, Is.EqualTo(new[] { 1.1d, 2.2d }));
        }

        [Test]
        public void ToList_Wrapper_CoversPrimitiveOverloads()
        {
            List<int> ints = 1.ToList(2, 3);
            List<short> shorts = ((short)1).ToList(2, 3);
            List<long> longs = 1L.ToList(2L, 3L);
            List<float> floats = 1.0f.ToList(2.0f);

            Assert.That(ints, Is.EqualTo(new List<int> { 1, 2, 3 }));
            Assert.That(shorts, Is.EqualTo(new List<short> { 1, 2, 3 }));
            Assert.That(longs, Is.EqualTo(new List<long> { 1L, 2L, 3L }));
            Assert.That(floats, Is.EqualTo(new List<float> { 1.0f, 2.0f }));
        }

        [Test]
        public void ToSelectList_CoversSelectionByValueAndNameAndNullTable()
        {
            DataTable table = new DataTable();
            table.Columns.Add("Name");
            table.Columns.Add("Value");
            table.Rows.Add("One", "1");
            table.Rows.Add("Two", "2");

            List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> byValue = table.ToSelectList("Name", "Value", selectedValue: "2");
            List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> byName = table.ToSelectList("Name", "Value", selectedName: "One");
            List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> fromNull = ((DataTable?)null).ToSelectList("Name", "Value");

            Assert.That(byValue.Count(i => i.Selected), Is.EqualTo(1));
            Assert.That(byValue.Single(i => i.Selected).Value, Is.EqualTo("2"));
            Assert.That(byName.Count(i => i.Selected), Is.EqualTo(1));
            Assert.That(byName.Single(i => i.Selected).Text, Is.EqualTo("One"));
            Assert.That(fromNull, Is.Empty);
        }
    }
}
