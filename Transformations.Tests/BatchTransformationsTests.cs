namespace Transformations.Tests
{
    using System;
    using System.Linq;

    using NUnit.Framework;

    [TestFixture]
    public class BatchTransformationsTests
    {
        [Test]
        public void BatchConvert_MixedInput_UsesFallbackOnInvalidItems()
        {
            object?[] source = { "1", 2, null, "bad", DBNull.Value, 5L };

            var result = BatchTransformations.BatchConvert(source, -1).ToList();

            Assert.That(result, Is.EqualTo(new[] { 1, 2, -1, -1, -1, 5 }));
        }

        [Test]
        public void BatchTransformInPlace_TitleCase_TransformsSpanEntries()
        {
            string?[] values = { "hello world", "SECOND VALUE", null };
            Span<string?> span = values.AsSpan();

            BatchTransformations.BatchTransformInPlace(span, BatchTransformations.BatchStringTransformation.ToTitleCase);

            Assert.That(values[0], Is.EqualTo("Hello World"));
            // TextInfo.ToTitleCase keeps fully-uppercase words as-is; this assertion captures that framework quirk explicitly.
            Assert.That(values[1], Is.EqualTo("SECOND VALUE"));
            Assert.That(values[2], Is.EqualTo(string.Empty));
        }

        [Test]
        public void BatchTransformInPlace_StripHtml_RemovesTags()
        {
            string?[] values = { "<b>Hello</b>", "<div>World</div>" };

            BatchTransformations.BatchTransformInPlace(values.AsSpan(), BatchTransformations.BatchStringTransformation.StripHtml);

            Assert.That(values[0], Is.EqualTo("Hello"));
            Assert.That(values[1], Is.EqualTo("World"));
        }

        [Test]
        public void BatchTransform_ReadOnlySpan_UsesPooledBufferAndReturnsArray()
        {
            string?[] source = { "<i>hello</i>", null, "test" };

            string[] result = BatchTransformations.BatchTransform(source.AsSpan(), BatchTransformations.BatchStringTransformation.StripHtml);

            Assert.That(result, Is.EqualTo(new[] { "hello", string.Empty, "test" }));
            // Input immutability check guards against future optimizations that accidentally mutate source while using pooled buffers.
            Assert.That(source[0], Is.EqualTo("<i>hello</i>"));
        }
    }
}
