using FictionalBookLending.src.Domain.ValueObjects.Book;
using FluentAssertions;

namespace FinctionalBookLending.Test
{
    [TestFixture]
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Parse_Should_Return_Valid_ISBN_When_Exactly_12_Digits()
        {
            // Arrange
            var validValue = "123456789012";

            // Act
            var isbn = ISBN.Parse(validValue);

            // Assert
            isbn.Value.Should().Be(validValue);
            isbn.IsValid.Should().BeTrue();
        }

        [Test]
        public void Parse_Should_Throw_When_Less_Than_12_Digits()
        {
            // Arrange
            var shortValue = "1234567";

            // Act
            Action act = () => ISBN.Parse(shortValue);

            // Assert
            act.Should().Throw<ArgumentException>()
               .WithMessage("*must be 12 digits*");
        }

        [Test]
        public void Parse_Should_Throw_When_More_Than_12_Digits()
        {
            // Arrange
            var longValue = "123456789012345";

            // Act
            Action act = () => ISBN.Parse(longValue);

            // Assert
            act.Should().Throw<ArgumentException>()
               .WithMessage("*must be 12 digits*");
        }

        [Test]
        public void Parse_Should_Throw_When_Contains_Letters()
        {
            // Arrange
            var invalidValue = "1234A6789B12";

            // Act
            Action act = () => ISBN.Parse(invalidValue);

            // Assert
            act.Should().Throw<ArgumentException>()
               .WithMessage("*numeric only*");
        }

        [Test]
        public void Parse_Should_Throw_When_Contains_Special_Characters()
        {
            // Arrange
            var invalidValue = "12345-678912";

            // Act
            Action act = () => ISBN.Parse(invalidValue);

            // Assert
            act.Should().Throw<ArgumentException>()
               .WithMessage("*numeric only*");
        }

        [Test]
        public void Parse_Should_Trim_Whitespace_And_Validate_Correctly()
        {
            // Arrange
            var value = "   123456789012   ";

            // Act
            var isbn = ISBN.Parse(value);

            // Assert
            isbn.IsValid.Should().BeTrue();
            isbn.Value.Should().Be("123456789012");
        }

        [Test]
        public void Parse_Should_Throw_When_Empty()
        {
            // Arrange
            var emptyValue = "";

            // Act
            Action act = () => ISBN.Parse(emptyValue);

            // Assert
            act.Should().Throw<ArgumentException>()
               .WithMessage("*cannot be empty*");
        }

        [Test]
        public void ToString_Should_Return_Raw_Value()
        {
            // Arrange
            var isbn = new ISBN("123456789012");

            // Act
            var result = isbn.ToString();

            // Assert
            result.Should().Be("123456789012");
        }
    }
}