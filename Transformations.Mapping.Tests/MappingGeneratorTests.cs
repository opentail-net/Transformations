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

            User user = dto.ToUser();

            Assert.That(user.Id, Is.EqualTo(2));
            Assert.That(user.Name, Is.EqualTo("Bob"));
            Assert.That(user.Email, Is.EqualTo("bob@test.com"));
        }

        #endregion Same-type mapping

        #region MapFrom mapping

        [Test]
        public void ToProductDto_MapFrom_MapsCorrectly()
        {
            var product = new Product { Id = 10, Title = "Laptop" };

            ProductDto dto = product.ToProductDto();

            Assert.That(dto.Id, Is.EqualTo(10));
            Assert.That(dto.Title, Is.EqualTo("Laptop"));
        }

        #endregion MapFrom mapping

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

            Order order = summary.ToOrder();

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

            Employee employee = card.ToEmployee();

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

            CatalogItem item = brief.ToCatalogItem();

            Assert.That(item.Id, Is.EqualTo(11));
            Assert.That(item.Title, Is.EqualTo("Compact"));
        }

        [Test]
        public void FromDetailedItem_MultipleTargets_ReverseMapping_Works()
        {
            var detailed = new DetailedItem { Id = 12, Title = "Full", Description = "Complete description" };

            CatalogItem item = detailed.ToCatalogItem();

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

        #region Enum-to-int type mismatch — property skipped, not ConvertTo<T>

        [Test]
        public void ToStatusDto_EnumToIntProperty_SkippedAndStringMapped()
        {
            // If the generator emits ConvertTo<int>() for the enum Status property this won't compile.
            // With the fix it skips that property; Name still maps.
            var record = new StatusRecord { Status = RecordStatus.Inactive, Name = "Test" };

            StatusDto dto = record.ToStatusDto();

            Assert.That(dto.Name, Is.EqualTo("Test"));
            Assert.That(dto.Status, Is.EqualTo(0)); // skipped → default int value
        }

        #endregion Enum-to-int type mismatch — property skipped, not ConvertTo<T>

        #region Inherited properties mapped from base class

        [Test]
        public void ToPerson_InheritedProperties_MappedFromBaseClass()
        {
            var created = new DateTime(2025, 6, 1);
            var person = new Person { Id = 7, Name = "Carol", CreatedAt = created };

            PersonDto dto = person.ToPersonDto();

            Assert.That(dto.Id, Is.EqualTo(7));
            Assert.That(dto.Name, Is.EqualTo("Carol"));
            Assert.That(dto.CreatedAt, Is.Not.Empty); // DateTime.ToString() — inherited prop mapped
        }

        #endregion Inherited properties mapped from base class

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

        [Test]
        public void ToGenericBoxDto_GenericSourceClass_GeneratesValidPartialClass()
        {
            var box = new GenericBox<int> { Value = 42 };

            GenericBoxDto dto = box.ToGenericBoxDto();

            Assert.That(dto.Value, Is.EqualTo("42"));
        }

        [Test]
        public void ToNestedDto_NestedSourceClass_GeneratesInsideContainingPartialClass()
        {
            var source = new MappingContainer.NestedSource { Id = 17, Name = "Nested" };

            MappingContainer.NestedDto dto = source.ToNestedDto();

            Assert.That(dto.Id, Is.EqualTo(17));
            Assert.That(dto.Name, Is.EqualTo("Nested"));
        }

        #endregion Edge cases

        #region Collection mapping

        [Test]
        public void ToUserDtoList_MapsAllItems()
        {
            var users = new List<User>
            {
                new() { Id = 1, Name = "Alice", Email = "alice@test.com" },
                new() { Id = 2, Name = "Bob", Email = "bob@test.com" }
            };

            List<UserDto> dtos = users.ToUserDtoList();

            Assert.That(dtos, Has.Count.EqualTo(2));
            Assert.That(dtos[0].Name, Is.EqualTo("Alice"));
            Assert.That(dtos[1].Email, Is.EqualTo("bob@test.com"));
        }

        [Test]
        public void ToUserDtoArray_MapsAllItems()
        {
            var users = new[]
            {
                new User { Id = 3, Name = "Carol", Email = "carol@test.com" },
                new User { Id = 4, Name = "Dan", Email = "dan@test.com" }
            };

            UserDto[] dtos = users.ToUserDtoArray();

            Assert.That(dtos, Has.Length.EqualTo(2));
            Assert.That(dtos[0].Id, Is.EqualTo(3));
            Assert.That(dtos[1].Name, Is.EqualTo("Dan"));
        }

        [Test]
        public void ToUserDtoEnumerable_MapsLazily()
        {
            var users = new List<User>
            {
                new() { Id = 5, Name = "Erin", Email = "erin@test.com" }
            };

            IEnumerable<UserDto> dtos = users.ToUserDtoEnumerable();

            Assert.That(dtos.Single().Name, Is.EqualTo("Erin"));
        }

        [Test]
        public void ToUserDtoList_EmptySequence_ReturnsEmptyList()
        {
            var users = Array.Empty<User>();

            List<UserDto> dtos = users.ToUserDtoList();

            Assert.That(dtos, Is.Empty);
        }

        [Test]
        public void ToUserDtoList_NullSource_ThrowsArgumentNull()
        {
            List<User>? users = null;

            Assert.Throws<ArgumentNullException>(() => users!.ToUserDtoList());
        }

        [Test]
        public void ToUserDtoList_NullItem_PreservesNullItem()
        {
            var users = new List<User> { new() { Id = 6, Name = "Finn", Email = "finn@test.com" }, null! };

            List<UserDto> dtos = users.ToUserDtoList();

            Assert.That(dtos, Has.Count.EqualTo(2));
            Assert.That(dtos[0].Name, Is.EqualTo("Finn"));
            Assert.That(dtos[1], Is.Null);
        }

        [Test]
        public void ToBriefItemList_MultipleTargets_CollectionMethodUsesRequestedTarget()
        {
            var items = new List<CatalogItem>
            {
                new() { Id = 7, Title = "Short", Description = "Long" }
            };

            List<BriefItem> briefs = items.ToBriefItemList();

            Assert.That(briefs.Single().Title, Is.EqualTo("Short"));
        }

        [Test]
        public void ToNestedDtoList_NestedSourceClass_CollectionMethodWorks()
        {
            var sources = new List<MappingContainer.NestedSource>
            {
                new() { Id = 18, Name = "Nested list" }
            };

            List<MappingContainer.NestedDto> dtos = sources.ToNestedDtoList();

            Assert.That(dtos.Single().Name, Is.EqualTo("Nested list"));
        }

        [Test]
        public void ToGenericBoxDtoList_GenericSourceClass_InfersTypeParameters()
        {
            var boxes = new List<GenericBox<int>>
            {
                new() { Value = 19 }
            };

            List<GenericBoxDto> dtos = boxes.ToGenericBoxDtoList();

            Assert.That(dtos.Single().Value, Is.EqualTo("19"));
        }

        #endregion Collection mapping

        #region Queryable projection

        [Test]
        public void ProjectToUserDto_Queryable_ProjectsAllItems()
        {
            var users = new List<User>
            {
                new() { Id = 41, Name = "Iris", Email = "iris@test.com" },
                new() { Id = 42, Name = "Jon", Email = "jon@test.com" }
            }.AsQueryable();

            List<UserDto> dtos = users.ProjectToUserDto().ToList();

            Assert.That(dtos, Has.Count.EqualTo(2));
            Assert.That(dtos[0].Id, Is.EqualTo(41));
            Assert.That(dtos[0].Name, Is.EqualTo("Iris"));
            Assert.That(dtos[1].Email, Is.EqualTo("jon@test.com"));
        }

        [Test]
        public void ProjectToContactCard_RenamedProperties_ProjectsTargetNames()
        {
            var employees = new List<Employee>
            {
                new() { Name = "Kai Lane", Phone = "555-0101" }
            }.AsQueryable();

            ContactCard card = employees.ProjectToContactCard().Single();

            Assert.That(card.FullName, Is.EqualTo("Kai Lane"));
            Assert.That(card.PhoneNumber, Is.EqualTo("555-0101"));
        }

        [Test]
        public void ProjectToUserDto_NullSource_ThrowsArgumentNull()
        {
            IQueryable<User>? users = null;

            Assert.Throws<ArgumentNullException>(() => users!.ProjectToUserDto());
        }

        [Test]
        public void ProjectToOrderSummary_RuntimeOnlyConversions_ThrowsNotSupported()
        {
            var orders = new List<Order>().AsQueryable();

            NotSupportedException? exception = Assert.Throws<NotSupportedException>(() => orders.ProjectToOrderSummary());

            Assert.That(exception!.Message, Does.Contain("OrderId"));
            Assert.That(exception.Message, Does.Contain("Amount"));
            Assert.That(exception.Message, Does.Contain("CreatedAt"));
        }

        #endregion Queryable projection

        #region Update existing target

        [Test]
        public void MapTo_ExistingTarget_UpdatesMappedPropertiesAndReturnsSameInstance()
        {
            var user = new User { Id = 21, Name = "Grace", Email = "grace@test.com" };
            var target = new UserDto { Id = 1, Name = "Old", Email = "old@test.com" };

            UserDto result = user.MapTo(target);

            Assert.That(result, Is.SameAs(target));
            Assert.That(target.Id, Is.EqualTo(21));
            Assert.That(target.Name, Is.EqualTo("Grace"));
            Assert.That(target.Email, Is.EqualTo("grace@test.com"));
        }

        [Test]
        public void MapTo_NullExistingTarget_ThrowsArgumentNull()
        {
            var user = new User { Id = 22, Name = "Helen", Email = "helen@test.com" };

            Assert.Throws<ArgumentNullException>(() => user.MapTo(null!));
        }

        [Test]
        public void MapTo_PartialTarget_PreservesUnmappedTargetValues()
        {
            var source = new PartialSource { Id = 23, Name = "Visible" };
            var target = new PartialTarget { Id = 1, Name = "Old", RequiredDisplayText = "Keep me" };

            source.MapTo(target);

            Assert.That(target.Id, Is.EqualTo(23));
            Assert.That(target.Name, Is.EqualTo("Visible"));
            Assert.That(target.RequiredDisplayText, Is.EqualTo("Keep me"));
        }

        [Test]
        public void MapTo_DifferentTypes_AppliesConversion()
        {
            var order = new Order
            {
                OrderId = 24,
                Amount = 15.25m,
                CreatedAt = new DateTime(2026, 1, 2)
            };
            var target = new OrderSummary();

            order.MapTo(target);

            Assert.That(target.OrderId, Is.EqualTo("24"));
            Assert.That(target.Amount, Is.Not.Empty);
            Assert.That(target.CreatedAt, Is.Not.Empty);
        }

        [Test]
        public void MapTo_NullableSourceProperty_UpdatesTargetWithNull()
        {
            var source = new NullableProfile { Nickname = null, DisplayName = "No nickname" };
            var target = new NullableProfileDto { Nickname = "Existing", DisplayName = "Old" };

            source.MapTo(target);

            Assert.That(target.Nickname, Is.Null);
            Assert.That(target.DisplayName, Is.EqualTo("No nickname"));
        }

        [Test]
        public void MapTo_MultipleTargets_UpdatesRequestedTarget()
        {
            var item = new CatalogItem { Id = 25, Title = "Update", Description = "Preserved detail" };
            var target = new DetailedItem();

            item.MapTo(target);

            Assert.That(target.Id, Is.EqualTo(25));
            Assert.That(target.Title, Is.EqualTo("Update"));
            Assert.That(target.Description, Is.EqualTo("Preserved detail"));
        }

        #endregion Update existing target

        #region Explicit flattening

        [Test]
        public void ToCustomerOrderDto_SourcePath_MapsNestedProperties()
        {
            var order = new CustomerOrder
            {
                Id = 31,
                Customer = new Customer
                {
                    Name = "Contoso",
                    Address = new Address { City = "London" }
                }
            };

            CustomerOrderDto dto = order.ToCustomerOrderDto();

            Assert.That(dto.Id, Is.EqualTo(31));
            Assert.That(dto.CustomerName, Is.EqualTo("Contoso"));
            Assert.That(dto.City, Is.EqualTo("London"));
        }

        [Test]
        public void ToCustomerOrderDto_SourcePath_NullNestedObject_UsesEmptyString()
        {
            var order = new CustomerOrder { Id = 32, Customer = null };

            CustomerOrderDto dto = order.ToCustomerOrderDto();

            Assert.That(dto.Id, Is.EqualTo(32));
            Assert.That(dto.CustomerName, Is.EqualTo(string.Empty));
            Assert.That(dto.City, Is.EqualTo(string.Empty));
        }

        [Test]
        public void MapTo_SourcePath_UpdatesExistingTarget()
        {
            var order = new CustomerOrder
            {
                Id = 33,
                Customer = new Customer
                {
                    Name = "Fabrikam",
                    Address = new Address { City = "Paris" }
                }
            };
            var target = new CustomerOrderDto();

            order.MapTo(target);

            Assert.That(target.Id, Is.EqualTo(33));
            Assert.That(target.CustomerName, Is.EqualTo("Fabrikam"));
            Assert.That(target.City, Is.EqualTo("Paris"));
        }

        [Test]
        public void FromCustomerOrderDto_SourcePathMappings_AreOneWay()
        {
            var dto = new CustomerOrderDto { Id = 34, CustomerName = "Northwind", City = "Oslo" };

            CustomerOrder order = dto.ToCustomerOrder();

            Assert.That(order.Id, Is.EqualTo(34));
            Assert.That(order.Customer, Is.Null);
        }

        #endregion Explicit flattening

        #region Custom conversion hooks

        [Test]
        public void ToConversionDto_CustomConverters_MapFormattingParsingAndCustomObject()
        {
            var source = new ConversionSource
            {
                CreatedOn = new DateTime(2026, 7, 5),
                QuantityText = "42",
                CodeValue = new CodeValue { Value = "SKU-42" }
            };

            ConversionDto dto = source.ToConversionDto();

            Assert.That(dto.FormattedDate, Is.EqualTo("2026-07-05"));
            Assert.That(dto.Quantity, Is.EqualTo(42));
            Assert.That(dto.Code, Is.EqualTo("SKU-42"));
        }

        [Test]
        public void MapTo_CustomConverters_UpdatesExistingTarget()
        {
            var source = new ConversionSource
            {
                CreatedOn = new DateTime(2026, 8, 9),
                QuantityText = "7",
                CodeValue = new CodeValue { Value = "SKU-7" }
            };
            var target = new ConversionDto();

            source.MapTo(target);

            Assert.That(target.FormattedDate, Is.EqualTo("2026-08-09"));
            Assert.That(target.Quantity, Is.EqualTo(7));
            Assert.That(target.Code, Is.EqualTo("SKU-7"));
        }

        [Test]
        public void FromConversionDto_CustomConverterMappings_AreOneWay()
        {
            var dto = new ConversionDto { FormattedDate = "2026-01-01", Quantity = 5, Code = "SKU-5" };

            ConversionSource source = dto.ToConversionSource();

            Assert.That(source.CreatedOn, Is.EqualTo(default(DateTime)));
            Assert.That(source.QuantityText, Is.EqualTo(string.Empty));
            Assert.That(source.CodeValue.Value, Is.EqualTo(string.Empty));
        }

        #endregion Custom conversion hooks
    }
}
