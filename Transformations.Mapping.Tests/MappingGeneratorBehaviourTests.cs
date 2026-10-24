using NUnit.Framework;
using Transformations.Mapping;

namespace Transformations.Mapping.Tests
{
    // Models for MappingGeneratorBehaviourTests: the generator runs on this project, so these exercise the generated
    // code at run time, not only its compilation.

    public enum ReviewStatus { Draft, Published }

    public class ReviewDto
    {
        public string StatusLabel { get; set; } = string.Empty;
        public int? Count { get; set; }
        public int? Parsed { get; set; }
        public string Name { get; init; } = string.Empty;
        public int CustomerCount { get; set; }
        public int? MaybeCustomerCount { get; set; }
        public string @event { get; set; } = string.Empty;
    }

    public class ReviewCustomer
    {
        public int Count { get; set; }
    }

    [MapTo<ReviewDto>]
    public partial class Review
    {
        [MapProperty("StatusLabel", Converter = nameof(FormatStatus))]
        public ReviewStatus Status { get; set; }

        public int Count { get; set; }

        public string? Parsed { get; set; }

        public string Name { get; set; } = string.Empty;

        [MapProperty("CustomerCount", SourcePath = "Customer.Count")]
        [MapProperty("MaybeCustomerCount", SourcePath = "Customer.Count")]
        public ReviewCustomer? Customer { get; set; }

        public string @event { get; set; } = string.Empty;

        private static string FormatStatus(ReviewStatus status) => status == ReviewStatus.Published ? "Live" : "Draft";
    }

    [TestFixture]
    public class MappingGeneratorBehaviourTests
    {
        [Test]
        public void PrivateConverter_IsCalled()
        {
            var dto = new Review { Status = ReviewStatus.Published }.ToReviewDto();

            Assert.That(dto.StatusLabel, Is.EqualTo("Live"));
        }

        [Test]
        public void IntToNullableInt_IsAssigned()
        {
            Assert.That(new Review { Count = 7 }.ToReviewDto().Count, Is.EqualTo(7));
        }

        [TestCase("42", 42)]
        [TestCase("", null)]
        [TestCase(null, null)]
        public void StringToNullableInt_ConvertsOrStaysNull(string? value, int? expected)
        {
            Assert.That(new Review { Parsed = value }.ToReviewDto().Parsed, Is.EqualTo(expected));
        }

        [Test]
        public void SourcePathThroughANullMember_GivesDefaultOrNull()
        {
            var withCustomer = new Review { Customer = new ReviewCustomer { Count = 5 } }.ToReviewDto();
            var withoutCustomer = new Review { Customer = null }.ToReviewDto();

            Assert.That(withCustomer.CustomerCount, Is.EqualTo(5));
            Assert.That(withCustomer.MaybeCustomerCount, Is.EqualTo(5));
            Assert.That(withoutCustomer.CustomerCount, Is.EqualTo(0));
            Assert.That(withoutCustomer.MaybeCustomerCount, Is.Null);
        }

        [Test]
        public void InitOnlyTarget_IsSetOnCreation_AndLeftAloneByMapTo()
        {
            var created = new Review { Name = "first" }.ToReviewDto();
            var updated = new Review { Name = "second", Count = 3 }.MapTo(created);

            Assert.That(created.Name, Is.EqualTo("first"));
            Assert.That(updated.Name, Is.EqualTo("first"), "init-only members are not updated");
            Assert.That(updated.Count, Is.EqualTo(3));
        }

        [Test]
        public void KeywordNamedProperty_IsMapped()
        {
            Assert.That(new Review { @event = "launch" }.ToReviewDto().@event, Is.EqualTo("launch"));
        }

        [Test]
        public void FromFactory_OnTheSource_MapsBack()
        {
            var back = Review.FromReviewDto(new ReviewDto { Count = 9, @event = "x" });

            Assert.That(back.@event, Is.EqualTo("x"));
        }
    }
}
