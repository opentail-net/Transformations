using NUnit.Framework;

namespace Transformations.Mapping.Tests.Features
{
    /// <summary>
    /// Runs the code generated for enums, nested objects and collections (models in FeatureModels.cs).
    /// </summary>
    [TestFixture]
    public class MappingGeneratorFeatureTests
    {
        // ── Enums ────────────────────────────────────────────────────────────

        private static EnumSource EnumSourceWith(Action<EnumSource>? configure = null)
        {
            var source = new EnumSource { ParsedText = "Red", Mood = Mood.Sad };
            configure?.Invoke(source);
            return source;
        }

        [TestCase(Level.Low, LevelDto.Low)]
        [TestCase(Level.High, LevelDto.High)]
        public void EnumsWithIdenticalMembers_AreCast(Level level, LevelDto expected)
        {
            Assert.That(EnumSourceWith(s => s.Level = level).ToEnumTarget().Level, Is.EqualTo(expected));
        }

        [TestCase(Level.Low, 0)]
        [TestCase(Level.High, 1)]
        public void EnumToNumber_IsCast(Level level, int expected)
        {
            Assert.That(EnumSourceWith(s => s.AsNumber = level).ToEnumTarget().Number, Is.EqualTo(expected));
        }

        [TestCase(0, Level.Low)]
        [TestCase(1, Level.High)]
        public void NumberToEnum_IsCast(int number, Level expected)
        {
            Assert.That(EnumSourceWith(s => s.Num = number).ToEnumTarget().FromNumber, Is.EqualTo(expected));
        }

        [TestCase(Color.Red, Shade.Red)]
        [TestCase(Color.Green, Shade.Green)]
        public void EnumsWithDifferentMembers_AreMappedByName(Color color, Shade expected)
        {
            Assert.That(EnumSourceWith(s => s.Shade = color).ToEnumTarget().Shade, Is.EqualTo(expected));
        }

        [Test]
        public void EnumMemberWithNoCounterpart_Throws()
        {
            var source = EnumSourceWith(s => s.Shade = Color.Blue);

            Assert.Throws<ArgumentOutOfRangeException>(() => source.ToEnumTarget());
        }

        [Test]
        public void EnumsMatchedByNameIgnoringCase()
        {
            Assert.That(EnumSourceWith(s => s.Mood = Mood.happy).ToEnumTarget().Mood, Is.EqualTo(MoodDto.HAPPY));
            Assert.That(EnumSourceWith(s => s.Mood = Mood.Sad).ToEnumTarget().Mood, Is.EqualTo(MoodDto.Sad));
        }

        [Test]
        public void NullableEnumToNullableEnum_KeepsNull()
        {
            Assert.That(EnumSourceWith(s => s.MaybeShade = null).ToEnumTarget().MaybeShade, Is.Null);
            Assert.That(EnumSourceWith(s => s.MaybeShade = Color.Green).ToEnumTarget().MaybeShade, Is.EqualTo(Shade.Green));
            Assert.That(EnumSourceWith(s => s.MaybeLevel = null).ToEnumTarget().MaybeLevel, Is.Null);
            Assert.That(EnumSourceWith(s => s.MaybeLevel = Level.High).ToEnumTarget().MaybeLevel, Is.EqualTo(LevelDto.High));
        }

        [Test]
        public void EnumToString_UsesTheMemberName()
        {
            Assert.That(EnumSourceWith(s => s.AsLabel = Level.High).ToEnumTarget().Label, Is.EqualTo("High"));
        }

        [TestCase("Green", Color.Green)]
        [TestCase("green", Color.Green)]
        [TestCase("", Color.Red)]
        public void StringToEnum_IsParsed_IgnoringCase(string text, Color expected)
        {
            Assert.That(EnumSourceWith(s => s.ParsedText = text).ToEnumTarget().Parsed, Is.EqualTo(expected));
        }

        [Test]
        public void StringToEnum_WithAnUnknownName_Throws()
        {
            Assert.Throws<ArgumentException>(() => EnumSourceWith(s => s.ParsedText = "Purple").ToEnumTarget());
        }

        [TestCase(null, null)]
        [TestCase("", null)]
        [TestCase("Blue", Color.Blue)]
        public void StringToNullableEnum_NullOrEmptyIsNull(string? text, Color? expected)
        {
            Assert.That(EnumSourceWith(s => s.MaybeParsedText = text).ToEnumTarget().MaybeParsed, Is.EqualTo(expected));
        }

        [Test]
        public void EnumMappings_RoundTripThroughTheReverseFactory()
        {
            var original = EnumSourceWith(s =>
            {
                s.Level = Level.High;
                s.ParsedText = "Green";
                s.Shade = Color.Green;
                s.Mood = Mood.Sad;
                s.MaybeLevel = Level.Low;
            });

            var back = EnumSource.FromEnumTarget(original.ToEnumTarget());

            Assert.That(back.Level, Is.EqualTo(Level.High));
            Assert.That(back.ParsedText, Is.EqualTo("Green"));
            Assert.That(back.Shade, Is.EqualTo(Color.Green));
            Assert.That(back.Mood, Is.EqualTo(Mood.Sad));
            Assert.That(back.MaybeLevel, Is.EqualTo(Level.Low));
        }

        // ── Nested objects ───────────────────────────────────────────────────

        [Test]
        public void NestedObject_IsMappedProperty_ByProperty()
        {
            var order = new Order { Id = 7, Customer = new Customer { Name = "Ada", Address = new Address { City = "Paris", Zip = "75" } } };

            var dto = order.ToOrderDto();

            Assert.That(dto.Id, Is.EqualTo(7));
            Assert.That(dto.Customer.Name, Is.EqualTo("Ada"));
            Assert.That(dto.Customer.Address!.City, Is.EqualTo("Paris"));
            Assert.That(dto.Customer.Address.Zip, Is.EqualTo("75"));

            order.Customer.Name = "changed afterwards";
            Assert.That(dto.Customer.Name, Is.EqualTo("Ada"), "the nested object is copied, not shared");
        }

        [Test]
        public void NullNestedObject_StaysNull()
        {
            var dto = new Order { Customer = new Customer { Address = null }, ShipTo = null }.ToOrderDto();

            Assert.That(dto.Customer.Address, Is.Null);
            Assert.That(dto.ShipTo, Is.Null);
        }

        [Test]
        public void NestedStruct_IsMapped_AndNullableStructKeepsNull()
        {
            var withValue = new Order { Origin = new Point { X = 1, Y = 2 }, Destination = new Point { X = 3, Y = 4 } }.ToOrderDto();
            var withoutValue = new Order { Destination = null }.ToOrderDto();

            Assert.That(withValue.Origin.X, Is.EqualTo(1));
            Assert.That(withValue.Origin.Y, Is.EqualTo(2));
            Assert.That(withValue.Destination!.Value.Y, Is.EqualTo(4));
            Assert.That(withoutValue.Destination, Is.Null);
        }

        [Test]
        public void NestedObjectProperties_AreConvertedToo()
        {
            var order = new Order { Lines = new List<LineItem> { new LineItem { Sku = "a", Quantity = 2, Priority = Level.High } } };

            Assert.That(order.ToOrderDto().Lines[0].Priority, Is.EqualTo(LevelDto.High));
        }

        // ── Collections ──────────────────────────────────────────────────────

        [Test]
        public void ListOfObjects_IsMappedElementByElement()
        {
            var order = new Order { Lines = new List<LineItem> { new LineItem { Sku = "a", Quantity = 1 }, new LineItem { Sku = "b", Quantity = 2 } } };

            var lines = order.ToOrderDto().Lines;

            Assert.That(lines.Select(l => l.Sku), Is.EqualTo(new[] { "a", "b" }));
            Assert.That(lines.Select(l => l.Quantity), Is.EqualTo(new[] { 1, 2 }));
        }

        [Test]
        public void EmptyListAndNullList()
        {
            var dto = new Order { Lines = new List<LineItem>(), OptionalLines = null }.ToOrderDto();

            Assert.That(dto.Lines, Is.Empty);
            Assert.That(dto.OptionalLines, Is.Null);
        }

        [Test]
        public void NullElement_StaysNull()
        {
            var dto = new Order { Lines = new List<LineItem> { null! } }.ToOrderDto();

            Assert.That(dto.Lines, Has.Count.EqualTo(1));
            Assert.That(dto.Lines[0], Is.Null);
        }

        [Test]
        public void ArrayToArray_AndListToArray()
        {
            var order = new Order
            {
                Packed = new[] { new LineItem { Sku = "p" } },
                AsArray = new List<LineItem> { new LineItem { Sku = "x" }, new LineItem { Sku = "y" } },
            };

            var dto = order.ToOrderDto();

            Assert.That(dto.Packed.Select(l => l.Sku), Is.EqualTo(new[] { "p" }));
            Assert.That(dto.AsArray.Select(l => l.Sku), Is.EqualTo(new[] { "x", "y" }));
        }

        [Test]
        public void EnumerableOfInt_ToListOfLong_WidensEachElement()
        {
            var order = new Order { Numbers = Enumerable.Range(1, 3) };

            Assert.That(order.ToOrderDto().Numbers, Is.EqualTo(new long[] { 1, 2, 3 }));
        }

        [Test]
        public void HashSetOfEnums_IsMapped()
        {
            var order = new Order { Levels = new HashSet<Level> { Level.High } };

            Assert.That(order.ToOrderDto().Levels, Is.EquivalentTo(new[] { LevelDto.High }));
        }

        [Test]
        public void ListOfEnums_ToListOfStrings()
        {
            var order = new Order { Priorities = new List<Level> { Level.High, Level.Low } };

            Assert.That(order.ToOrderDto().Priorities, Is.EqualTo(new[] { "High", "Low" }));
        }

        [Test]
        public void DictionaryValues_AreMapped()
        {
            var order = new Order { ByKey = new Dictionary<string, LineItem> { ["one"] = new LineItem { Sku = "s1" } } };

            var dto = order.ToOrderDto();

            Assert.That(dto.ByKey.Keys, Is.EqualTo(new[] { "one" }));
            Assert.That(dto.ByKey["one"].Sku, Is.EqualTo("s1"));
        }

        [Test]
        public void ListOfLists_IsMappedRecursively()
        {
            var order = new Order { Grid = new List<List<LineItem>> { new List<LineItem> { new LineItem { Sku = "g" } }, new List<LineItem>() } };

            var grid = order.ToOrderDto().Grid;

            Assert.That(grid, Has.Count.EqualTo(2));
            Assert.That(grid[0][0].Sku, Is.EqualTo("g"));
            Assert.That(grid[1], Is.Empty);
        }

        [Test]
        public void ReverseFactory_MapsNestedObjectsAndCollectionsBack()
        {
            var dto = new Order
            {
                Id = 3,
                Customer = new Customer { Name = "Bo", Address = new Address { City = "Rome" } },
                Lines = new List<LineItem> { new LineItem { Sku = "z", Quantity = 5, Priority = Level.High } },
            }.ToOrderDto();

            var back = Order.FromOrderDto(dto);

            Assert.That(back.Id, Is.EqualTo(3));
            Assert.That(back.Customer.Address!.City, Is.EqualTo("Rome"));
            Assert.That(back.Lines.Single().Sku, Is.EqualTo("z"));
            Assert.That(back.Lines.Single().Priority, Is.EqualTo(Level.High));
        }

        [Test]
        public void MapToExisting_ReplacesNestedObjects()
        {
            var existing = new OrderDto { Customer = new CustomerDto { Name = "old" } };

            new Order { Customer = new Customer { Name = "new" } }.MapTo(existing);

            Assert.That(existing.Customer.Name, Is.EqualTo("new"));
        }

        // ── A type that refers to itself ─────────────────────────────────────

        [Test]
        public void SelfReferencingType_IsMappedToItsDepth()
        {
            var tree = new Node
            {
                Name = "root",
                Next = new Node { Name = "next" },
                Children = new List<Node>
                {
                    new Node { Name = "a", Children = new List<Node> { new Node { Name = "a1" } } },
                    new Node { Name = "b" },
                },
            };

            var dto = tree.ToNodeDto();

            Assert.That(dto.Name, Is.EqualTo("root"));
            Assert.That(dto.Next!.Name, Is.EqualTo("next"));
            Assert.That(dto.Children.Select(c => c.Name), Is.EqualTo(new[] { "a", "b" }));
            Assert.That(dto.Children[0].Children.Single().Name, Is.EqualTo("a1"));
            Assert.That(dto.Children[0].Children.Single().Next, Is.Null);
        }

        [Test]
        public void SelfReferencingType_CannotBeProjected_AndSaysWhy()
        {
            var ex = Assert.Throws<NotSupportedException>(() => new[] { new Node() }.AsQueryable().ProjectToNodeDto());

            Assert.That(ex!.Message, Does.Contain("Next").And.Contain("Children"));
        }

        [Test]
        public void SelfReferencingType_ReverseFactory()
        {
            var dto = new Node { Name = "r", Children = new List<Node> { new Node { Name = "c" } } }.ToNodeDto();

            var back = Node.FromNodeDto(dto);

            Assert.That(back.Children.Single().Name, Is.EqualTo("c"));
        }

        // ── Query projection ─────────────────────────────────────────────────

        private static IQueryable<ShopOrder> ShopOrders() => new[]
        {
            new ShopOrder
            {
                Id = 1,
                Customer = new Customer { Name = "Ada", Address = new Address { City = "Paris", Zip = "75" } },
                ShipTo = new Address { City = "Rome" },
                Origin = new Point { X = 9, Y = 8 },
                Destination = new Point { X = 1, Y = 2 },
                Lines = new List<LineItem> { new LineItem { Sku = "a", Quantity = 2, Priority = Level.High }, new LineItem { Sku = "b", Quantity = 1 } },
                Packed = new[] { new LineItem { Sku = "p", Quantity = 7 } },
                Priority = Level.High,
                AsNumber = Level.High,
            },
            new ShopOrder { Id = 2, Customer = new Customer { Name = "Bo", Address = null }, ShipTo = null, Destination = null },
        }.AsQueryable();

        [Test]
        public void ProjectTo_GivesTheSameResultAsTheMappingMethod()
        {
            var orders = ShopOrders();

            var projected = orders.ProjectToShopOrderDto().ToList();
            var mapped = orders.ToList().Select(o => o.ToShopOrderDto()).ToList();

            Assert.That(projected, Has.Count.EqualTo(2));
            for (int i = 0; i < projected.Count; i++)
            {
                Assert.That(projected[i].Id, Is.EqualTo(mapped[i].Id));
                Assert.That(projected[i].Customer.Name, Is.EqualTo(mapped[i].Customer.Name));
                Assert.That(projected[i].Customer.Address?.City, Is.EqualTo(mapped[i].Customer.Address?.City));
                Assert.That(projected[i].ShipTo?.City, Is.EqualTo(mapped[i].ShipTo?.City));
                Assert.That(projected[i].Origin.X, Is.EqualTo(mapped[i].Origin.X));
                Assert.That(projected[i].Destination?.Y, Is.EqualTo(mapped[i].Destination?.Y));
                Assert.That(projected[i].Lines.Select(l => (l.Sku, l.Quantity, l.Priority)), Is.EqualTo(mapped[i].Lines.Select(l => (l.Sku, l.Quantity, l.Priority))));
                Assert.That(projected[i].Packed.Select(l => l.Sku), Is.EqualTo(mapped[i].Packed.Select(l => l.Sku)));
                Assert.That(projected[i].Priority, Is.EqualTo(mapped[i].Priority));
                Assert.That(projected[i].PriorityNumber, Is.EqualTo(mapped[i].PriorityNumber));
            }
        }

        [Test]
        public void ProjectTo_InlinesNestedObjectsIntoOneMemberInitialiser()
        {
            var source = ShopOrders().ProjectToShopOrderDto();

            string expression = source.Expression.ToString();

            Assert.That(expression, Does.Contain("new CustomerDto"), "the nested object is projected inline, not through a helper call");
            Assert.That(expression, Does.Not.Contain("__Map"));
            Assert.That(expression, Does.Contain("ToList"));
        }
    }
}
