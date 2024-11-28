using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Constants;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Models;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Create;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Unit.Builders;
using Moq;
using Moq.AutoMock;
using Xunit;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Unit.Create;

public class CreateManagerTests
{
    private readonly AutoMocker _mocker;
    private readonly CreateManager _sut;
    private readonly CurrencyRateEntity _currencyRateEntity;

    public CreateManagerTests()
    {
        _mocker = new AutoMocker();
        _sut = _mocker.CreateInstance<CreateManager>();

        _currencyRateEntity = new CurrencyRateBuilder().BuildEntity();

        _mocker.GetMock<IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity>>()
            .Setup(h => h.CreateAsync(It.IsAny<CurrencyRateEntity>(), It.IsAny<BaseRevisionEntity?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EntityLifeCycleResult<CurrencyRateEntity, BaseRevisionEntity>
            {
                Entity = _currencyRateEntity,
                Revision = new BaseRevisionEntity()
            });
    }

    [Fact]
    public async Task CreateAsync_ShouldCallEntityLifecycleHandlerCreateAsync()
    {
        // Act
        await _sut.CreateAsync(_currencyRateEntity);

        // Assert
        _mocker.GetMock<IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity>>()
            .Verify(h => h.CreateAsync(_currencyRateEntity, null, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnSuccessfulManagerResponse()
    {
        // Act
        var result = await _sut.CreateAsync(_currencyRateEntity);

        // Assert
        Assert.Equal(ManagerResponseState.Success, result.State);
        Assert.Equal(_currencyRateEntity.OriginKey, result.CurrencyRate?.Entity.OriginKey);
        Assert.Equal(_currencyRateEntity.TargetKey, result.CurrencyRate?.Entity.TargetKey);
        Assert.Equal(_currencyRateEntity.Rate, result.CurrencyRate?.Entity.Rate);
    }
}