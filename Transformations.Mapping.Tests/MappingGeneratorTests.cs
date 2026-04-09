using NUnit.Framework;

namespace Transformations.Mapping.Tests
{
    [TestFixture]
    public class MappingGeneratorTests
    {
        #region Same-type mapping

        [Test]
        public void ToUserDto_SameTypeProperties_MapsCorrectly()
        {
            var user = new User { Id = 1, Name = "Alice", Email = "alice@test.com" };

            UserDto dto = user.ToUserDto();

            Assert.That(dto.Id, Is.EqualTo(1));
            Assert.That(dto.Name, Is.EqualTo("Alice"));
            Assert.That(dto.Email, Is.EqualTo("alice@test.com"));
        }

        [Test]
        public void FromUserDto_ReverseMapping_MapsCorrectly()
        {
            var dto = new UserDto { Id = 2, Name = "Bob", Email = "bob@test.com" };

            User user = User.FromUserDto(dto);

            Assert.That(user.Id, Is.EqualTo(2));
            Assert.That(user.Name, Is.EqualTo("Bob"));
            Assert.That(user.Email, Is.EqualTo("bob@test.com"));
        }

        #endregion Same-type mapping

        #region Type conversion mapping

        [Test]
        public void ToOrderSummary_DifferentTypes_ConvertsViaToString()
        {
            var order = new Order
            {
                OrderId = 42,
                Amount = 99.95m,
                CreatedAt = new DateTime(2025, 1, 15)
            };

            OrderSummary summary = order.ToOrderSummary();

            Assert.That(summary.OrderId, Is.EqualTo("42"));
            Assert.That(summary.Amount, Is.Not.Empty);
            Assert.That(summary.CreatedAt, Is.Not.Empty);
        }

        [Test]
        public void FromOrderSummary_ReverseMapping_ConvertsBackToNativeTypes()
        {
            var summary = new OrderSummary
            {
                OrderId = "77",
                Amount = "123.45",
                CreatedAt = "2025-01-20"
            };

            Order order = Order.FromOrderSummary(summary);

            Assert.That(order.OrderId, Is.EqualTo(77));
            Assert.That(order.Amount, Is.EqualTo(123.45m));
            Assert.That(order.CreatedAt, Is.EqualTo(new DateTime(2025, 1, 20)));
        }

        #endregion Type conversion mapping

        #region IgnoreMap

        [Test]
        public void ToPublicProfile_IgnoredProperty_NotMapped()
        {
            var user = new UserWithSecrets
            {
                Name = "Alice",
                Bio = "Developer",
                PasswordHash = "secret123"
            };

            PublicProfile profile = user.ToPublicProfile();

            Assert.That(profile.Name, Is.EqualTo("Alice"));
            Assert.That(profile.Bio, Is.EqualTo("Developer"));
        }

        #endregion IgnoreMap

        #region MapProperty rename

        [Test]
        public void ToContactCard_RenamedProperties_MapsCorrectly()
        {
            var employee = new Employee { Name = "Alice Smith", Phone = "555-1234" };

            ContactCard card = employee.ToContactCard();

            Assert.That(card.FullName, Is.EqualTo("Alice Smith"));
            Assert.That(card.PhoneNumber, Is.EqualTo("555-1234"));
        }

        [Test]
        public void FromContactCard_RenamedProperties_ReverseMapsCorrectly()
        {
            var card = new ContactCard { FullName = "Bob Jones", PhoneNumber = "555-5678" };

            Employee employee = Employee.FromContactCard(card);

            Assert.That(employee.Name, Is.EqualTo("Bob Jones"));
            Assert.That(employee.Phone, Is.EqualTo("555-5678"));
        }

        #endregion MapProperty rename

        #region Multiple targets

        [Test]
        public void ToBriefItem_MultipleTargets_FirstTargetWorks()
        {
            var item = new CatalogItem { Id = 1, Title = "Widget", Description = "A fine widget" };

            BriefItem brief = item.ToBriefItem();

            Assert.That(brief.Id, Is.EqualTo(1));
            Assert.That(brief.Title, Is.EqualTo("Widget"));
        }

        [Test]
        public void ToDetailedItem_MultipleTargets_SecondTargetWorks()
        {
            var item = new CatalogItem { Id = 2, Title = "Gadget", Description = "A cool gadget" };

            DetailedItem detailed = item.ToDetailedItem();

            Assert.That(detailed.Id, Is.EqualTo(2));
            Assert.That(detailed.Title, Is.EqualTo("Gadget"));
            Assert.That(detailed.Description, Is.EqualTo("A cool gadget"));
        }

        [Test]
        public void FromBriefItem_MultipleTargets_ReverseMapping_Works()
        {
            var brief = new BriefItem { Id = 11, Title = "Compact" };

            CatalogItem item = CatalogItem.FromBriefItem(brief);

            Assert.That(item.Id, Is.EqualTo(11));
            Assert.That(item.Title, Is.EqualTo("Compact"));
        }

        [Test]
        public void FromDetailedItem_MultipleTargets_ReverseMapping_Works()
        {
            var detailed = new DetailedItem { Id = 12, Title = "Full", Description = "Complete description" };

            CatalogItem item = CatalogItem.FromDetailedItem(detailed);

            Assert.That(item.Id, Is.EqualTo(12));
            Assert.That(item.Title, Is.EqualTo("Full"));
            Assert.That(item.Description, Is.EqualTo("Complete description"));
        }

        #endregion Multiple targets

        #region Unmapped target-member diagnostic example behavior

        [Test]
        public void ToPartialTarget_MappedMembersStillWork_WhenTargetHasUnmappedMembers()
        {
            var source = new PartialSource { Id = 5, Name = "Visible" };

            PartialTarget target = source.ToPartialTarget();

            Assert.That(target.Id, Is.EqualTo(5));
            Assert.That(target.Name, Is.EqualTo("Visible"));
            Assert.That(target.RequiredDisplayText, Is.EqualTo(string.Empty));
        }

        #endregion Unmapped target-member diagnostic example behavior

        #region Edge cases

        [TestCase("", "", "")]
        [TestCase("Alice", "alice@test.com", "Alice")]
        public void ToUserDto_VariousInputs_HandlesCorrectly(string name, string email, string expectedName)
        {
            var user = new User { Id = 1, Name = name, Email = email };

            UserDto dto = user.ToUserDto();

            Assert.That(dto.Name, Is.EqualTo(expectedName));
        }

        [Test]
        public void ToUserDto_DefaultValues_MapsDefaults()
        {
            var user = new User();

            UserDto dto = user.ToUserDto();

            Assert.That(dto.Id, Is.EqualTo(0));
            Assert.That(dto.Name, Is.EqualTo(string.Empty));
        }

        #endregion Edge cases
    }
}
