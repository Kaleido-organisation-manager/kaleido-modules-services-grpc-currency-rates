using FluentValidation.TestHelper;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Validators;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Unit.Common.Validators;

public class KeyValidatorTests
{
    private readonly KeyValidator _sut;

    public KeyValidatorTests()
    {
        _sut = new KeyValidator();
    }

    [Fact]
    public void Validate_ValidGuid_ShouldNotHaveValidationError()
    {
        // Arrange
        var key = Guid.NewGuid().ToString();

        // Act
        var result = _sut.TestValidate(key);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_EmptyOrNullKey_ShouldHaveValidationError(string key)
    {
        // Act
        var result = _sut.TestValidate(key);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x);
    }

    [Fact]
    public void Validate_InvalidGuidFormat_ShouldHaveValidationError()
    {
        // Arrange
        var key = "not-a-guid";

        // Act
        var result = _sut.TestValidate(key);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x);
    }
}