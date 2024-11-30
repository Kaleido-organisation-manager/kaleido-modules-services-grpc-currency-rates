using FluentValidation.TestHelper;
using Kaleido.Grpc.CurrencyRates;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Validators;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Unit.Builders;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Unit.Common.Validators;

public class CurrencyRateValidatorTests
{
    private readonly CurrencyRateValidator _sut;
    private readonly CurrencyRate _validCurrencyRate;

    public CurrencyRateValidatorTests()
    {
        _sut = new CurrencyRateValidator();
        _validCurrencyRate = new CurrencyRateBuilder().Build();
    }

    [Fact]
    public void Validate_ValidCurrencyRate_ShouldNotHaveValidationError()
    {
        // Act
        var result = _sut.TestValidate(_validCurrencyRate);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid-guid")]
    public void Validate_InvalidOriginKey_ShouldHaveValidationError(string originKey)
    {
        // Arrange
        _validCurrencyRate.OriginKey = originKey;

        // Act
        var result = _sut.TestValidate(_validCurrencyRate);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.OriginKey);
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid-guid")]
    public void Validate_InvalidTargetKey_ShouldHaveValidationError(string targetKey)
    {
        // Arrange
        _validCurrencyRate.TargetKey = targetKey;

        // Act
        var result = _sut.TestValidate(_validCurrencyRate);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TargetKey);
    }

    [Fact]
    public void Validate_InvalidRate_ShouldHaveValidationError()
    {
        // Arrange
        _validCurrencyRate.Rate = -1;

        // Act
        var result = _sut.TestValidate(_validCurrencyRate);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Rate);
    }
}