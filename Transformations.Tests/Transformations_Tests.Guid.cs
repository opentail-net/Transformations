namespace Transformations.Tests
{
    using System;

    using NUnit.Framework;

    using Transformations;

    /// <summary>
    /// The basic type converter tests.
    /// </summary>
    [TestFixture]
    public partial class BasicTypeConverterTests
    {
        #region Methods

        [Test]
        public void ConvertToGuid_InvalidInput_ReturnsDefaultValue()
        {
            //// Setup
            string valueInput = "invalid input";
            Guid expected = Guid.Empty;

            //// Act
            Guid actual = valueInput.ConvertTo<Guid>(Guid.Empty);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToGuid_NullInput_ReturnsDefaultValue()
        {
            //// Setup
            string? valueInput = null;
            Guid expected = Guid.Empty;

            //// Act
            Guid actual = valueInput.ConvertTo<Guid>(Guid.Empty);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToGuid_ValidInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            string valueInput = "7F8C14B6-B3A8-4F71-8EFC-E5A7B35923B6";
            Guid expected = new Guid(valueInput);

            //// Act
            Guid actual = valueInput.ConvertTo<Guid>(Guid.Empty);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion Methods
    }
}