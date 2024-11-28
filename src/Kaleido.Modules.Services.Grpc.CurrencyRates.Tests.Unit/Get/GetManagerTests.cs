using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Constants;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Models;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Get;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Unit.Builders;
using Moq;
using Moq.AutoMock;
using Xunit;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Unit.Get;

public class GetManagerTests
{
    private readonly AutoMocker _mocker;
    private readonly GetManager _sut;
    private readonly Guid _validKey;
    private readonly EntityLifeCycleResult<CurrencyRateEntity, BaseRevisionEntity> _entityResult;

    public GetManagerTests()
    {
        _mocker = new AutoMocker();
        _validKey = Guid.NewGuid();

        var entity = new CurrencyRateBuilder().BuildEntity();
        _entityResult = new EntityLifeCycleResult<CurrencyRateEntity, BaseRevisionEntity>
        {
            Entity = entity,
            Revision = new BaseRevisionEntity
            {
                Key = _validKey,
                Action = RevisionAction.Created
            }
        };

        _mocker.GetMock<IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity>>()
            .Setup(h => h.GetAsync(_validKey, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_entityResult);

        _sut = _mocker.CreateInstance<GetManager>();
    }

    [Fact]
    public async Task GetAsync_ValidKey_CallsEntityLifecycleHandler()
    {
        // Act
        await _sut.GetAsync(_validKey);

        // Assert
        _mocker.GetMock<IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity>>()
            .Verify(h => h.GetAsync(_validKey, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAsync_ValidKey_ReturnsSuccessResponse()
    {
        // Act
        var result = await _sut.GetAsync(_validKey);

        // Assert
        Assert.Equal(ManagerResponseState.Success, result.State);
        Assert.Equal(_entityResult, result.CurrencyRate);
    }

    [Fact]
    public async Task GetAsync_EntityNotFound_ReturnsNotFoundResponse()
    {
        // Arrange
        _mocker.GetMock<IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity>>()
            .Setup(h => h.GetAsync(_validKey, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EntityLifeCycleResult<CurrencyRateEntity, BaseRevisionEntity>?)null);

        // Act
        var result = await _sut.GetAsync(_validKey);

        // Assert
        Assert.Equal(ManagerResponseState.NotFound, result.State);
    }

    [Fact]
    public async Task GetAsync_EntityDeleted_ReturnsNotFoundResponse()
    {
        // Arrange
        var deletedResult = new EntityLifeCycleResult<CurrencyRateEntity, BaseRevisionEntity>
        {
            Entity = _entityResult.Entity,
            Revision = new BaseRevisionEntity
            {
                Key = _validKey,
                Action = RevisionAction.Deleted
            }
        };

        _mocker.GetMock<IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity>>()
            .Setup(h => h.GetAsync(_validKey, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(deletedResult);

        // Act
        var result = await _sut.GetAsync(_validKey);

        // Assert
        Assert.Equal(ManagerResponseState.NotFound, result.State);
    }
}