using System.ComponentModel.DataAnnotations;

namespace Utilities;

[TestClass]
public class UrlSafe
{
    private readonly UrlSafeAttribute _attribute = new();

    [TestMethod]
    public void NullShouldBeValid()
    {
        // Arrange & Act
        bool isValid = _attribute.IsValid(null);

        // Assert
        isValid.Should().BeTrue();
    }

    [TestMethod]
    public void EmptyStringShouldBeValid()
    {
        // Arrange & Act
        bool isValid = _attribute.IsValid(string.Empty);

        // Assert
        isValid.Should().BeTrue();
    }

    [TestMethod]
    public void AlphanumericStringShouldBeValid()
    {
        // Arrange & Act
        bool isValid = _attribute.IsValid("abcDEF123");

        // Assert
        isValid.Should().BeTrue();
    }

    [TestMethod]
    public void StringWithAllowedSpecialCharsShouldBeValid()
    {
        // Arrange & Act
        bool isValid1 = _attribute.IsValid("test-name");
        bool isValid2 = _attribute.IsValid("test.name");
        bool isValid3 = _attribute.IsValid("test_name");
        bool isValid4 = _attribute.IsValid("test-name.with_chars");

        // Assert
        isValid1.Should().BeTrue();
        isValid2.Should().BeTrue();
        isValid3.Should().BeTrue();
        isValid4.Should().BeTrue();
    }

    [TestMethod]
    public void NonStringValueShouldBeInvalid()
    {
        // Arrange & Act
        bool isValid = _attribute.IsValid(123);

        // Assert
        isValid.Should().BeFalse();
    }

    [TestMethod]
    public void StringWithTildeShouldBeInvalid()
    {
        // Arrange & Act
        bool isValid = _attribute.IsValid("test~name");

        // Assert
        isValid.Should().BeFalse();
    }

    [TestMethod]
    public void StringWithSpaceShouldBeInvalid()
    {
        // Arrange & Act
        bool isValid = _attribute.IsValid("test name");

        // Assert
        isValid.Should().BeFalse();
    }

    [TestMethod]
    public void StringWithPlusShouldBeInvalid()
    {
        // Arrange & Act
        bool isValid = _attribute.IsValid("test+name");

        // Assert
        isValid.Should().BeFalse();
    }

    [TestMethod]
    public void StringWithPercentShouldBeInvalid()
    {
        // Arrange & Act
        bool isValid = _attribute.IsValid("test%20name");

        // Assert
        isValid.Should().BeFalse();
    }

    [TestMethod]
    public void StringWithPipeShouldBeInvalid()
    {
        // Arrange & Act
        bool isValid = _attribute.IsValid("test|name");

        // Assert
        isValid.Should().BeFalse();
    }

    [TestMethod]
    public void StringWithMultipleInvalidCharsShouldBeInvalid()
    {
        // Arrange & Act
        bool isValid = _attribute.IsValid("test@name#with!invalid&chars");

        // Assert
        isValid.Should().BeFalse();
    }

    [TestMethod]
    public void ValidationShouldWork()
    {
        // Arrange
        TestUrlSafeClass testObject = new() { UrlSafeName = "valid-name" };
        ValidationContext validationContext = new(testObject);
        List<ValidationResult> validationResults = [];

        // Act
        bool isValid = Validator.TryValidateObject(testObject, validationContext, validationResults, true);

        // Assert
        isValid.Should().BeTrue();
        validationResults.Should().BeEmpty();

        // Arrange - now with invalid value
        testObject.UrlSafeName = "invalid name with spaces";
        validationResults.Clear();

        // Act
        isValid = Validator.TryValidateObject(testObject, validationContext, validationResults, true);

        // Assert
        isValid.Should().BeFalse();
        validationResults.Should().HaveCount(1);
    }

    private class TestUrlSafeClass
    {
        [UrlSafe]
        public string UrlSafeName { get; set; } = string.Empty;
    }
}
