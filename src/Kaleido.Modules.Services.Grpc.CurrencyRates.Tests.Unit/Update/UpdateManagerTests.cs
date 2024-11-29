using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Exceptions;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Constants;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Models;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Unit.Builders;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Update;
using Moq;
using Moq.AutoMock;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Unit.Update;

public class UpdateManagerTests
{
    private readonly AutoMocker _mocker;
    private readonly UpdateManager _sut;
    private readonly Guid _validKey;
    private readonly CurrencyRateEntity _currencyRate;
    private readonly EntityLifeCycleResult<CurrencyRateEntity, BaseRevisionEntity> _entityResult;

    public UpdateManagerTests()
    {
        _mocker = new AutoMocker();
        _validKey = Guid.NewGuid();
        _currencyRate = new CurrencyRateBuilder().BuildEntity();

        _entityResult = new EntityLifeCycleResult<CurrencyRateEntity, BaseRevisionEntity>
        {
            Entity = _currencyRate,
            Revision = new BaseRevisionEntity
            {
                Key = _validKey,
                Action = RevisionAction.Updated
            }
        };

        _mocker.GetMock<IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity>>()
            .Setup(h => h.UpdateAsync(_validKey, It.IsAny<CurrencyRateEntity>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_entityResult);

        _sut = _mocker.CreateInstance<UpdateManager>();
    }

    [Fact]
    public async Task UpdateAsync_ValidRequest_CallsEntityLifecycleHandler()
    {
        // Act
        await _sut.UpdateAsync(_validKey, _currencyRate);

        // Assert
        _mocker.GetMock<IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity>>()
            .Verify(h => h.UpdateAsync(_validKey, _currencyRate, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ValidRequest_ReturnsSuccessResponse()
    {
        // Act
        var result = await _sut.UpdateAsync(_validKey, _currencyRate);

        // Assert
        Assert.Equal(ManagerResponseState.Success, result.State);
        Assert.Equal(_entityResult, result.CurrencyRate);
    }

    [Fact]
    public async Task UpdateAsync_EntityNotFound_ReturnsNotFoundResponse()
    {
        // Arrange
        _mocker.GetMock<IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity>>()
            .Setup(h => h.UpdateAsync(_validKey, It.IsAny<CurrencyRateEntity>(), null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException("Currency rate not found"));

        // Act
        var result = await _sut.UpdateAsync(_validKey, _currencyRate);

        // Assert
        Assert.Equal(ManagerResponseState.NotFound, result.State);
    }

    [Fact]
    public async Task UpdateAsync_RevisionNotFound_ReturnsNotFoundResponse()
    {
        // Arrange
        _mocker.GetMock<IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity>>()
            .Setup(h => h.UpdateAsync(_validKey, It.IsAny<CurrencyRateEntity>(), null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RevisionNotFoundException("Revision not found"));

        // Act
        var result = await _sut.UpdateAsync(_validKey, _currencyRate);

        // Assert
        Assert.Equal(ManagerResponseState.NotFound, result.State);
    }

    [Fact]
    public async Task UpdateAsync_InvalidOperation_ReturnsNotFoundResponse()
    {
        // Arrange
        _mocker.GetMock<IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity>>()
            .Setup(h => h.UpdateAsync(_validKey, It.IsAny<CurrencyRateEntity>(), null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException());

        // Act
        var result = await _sut.UpdateAsync(_validKey, _currencyRate);

        // Assert
        Assert.Equal(ManagerResponseState.NotFound, result.State);
    }

    [Fact]
    public async Task UpdateAsync_NotModified_ReturnsNotModifiedResponse()
    {
        // Arrange
        _mocker.GetMock<IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity>>()
            .Setup(h => h.UpdateAsync(_validKey, It.IsAny<CurrencyRateEntity>(), null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotModifiedException("Currency rate is up to date"));

        // Act
        var result = await _sut.UpdateAsync(_validKey, _currencyRate);

        // Assert
        Assert.Equal(ManagerResponseState.NotModified, result.State);
    }
}