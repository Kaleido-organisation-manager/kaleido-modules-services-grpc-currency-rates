using System.Linq.Expressions;
using Kaleido.Common.Services.Grpc.Constants;
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
    private readonly CurrencyRateEntity _currencyRate;
    private readonly EntityLifeCycleResult<CurrencyRateEntity, BaseRevisionEntity> _entityResult;

    public CreateManagerTests()
    {
        _mocker = new AutoMocker();
        _currencyRate = new CurrencyRateBuilder().BuildEntity();

        _entityResult = new EntityLifeCycleResult<CurrencyRateEntity, BaseRevisionEntity>
        {
            Entity = _currencyRate,
            Revision = new BaseRevisionEntity
            {
                Key = Guid.NewGuid(),
                Action = RevisionAction.Created
            }
        };

        // Setup for no existing entity (happy path)
        _mocker.GetMock<IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity>>()
            .Setup(h => h.FindAsync(
                It.IsAny<Expression<Func<CurrencyRateEntity, bool>>>(),
                It.IsAny<Expression<Func<BaseRevisionEntity, bool>>>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<EntityLifeCycleResult<CurrencyRateEntity, BaseRevisionEntity>>());

        _mocker.GetMock<IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity>>()
            .Setup(h => h.CreateAsync(_currencyRate, It.IsAny<BaseRevisionEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_entityResult);

        _sut = _mocker.CreateInstance<CreateManager>();
    }

    [Fact]
    public async Task CreateAsync_NoExistingEntity_CallsCreateOnEntityLifecycleHandler()
    {
        // Act
        await _sut.CreateAsync(_currencyRate);

        // Assert
        _mocker.GetMock<IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity>>()
            .Verify(h => h.CreateAsync(_currencyRate, It.IsAny<BaseRevisionEntity>(), CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_NoExistingEntity_ReturnsSuccessResponse()
    {
        // Act
        var result = await _sut.CreateAsync(_currencyRate);

        // Assert
        Assert.Equal(ManagerResponseState.Success, result.State);
        Assert.Equal(_entityResult, result.CurrencyRate);
    }

    [Fact]
    public async Task CreateAsync_ExistingActiveEntity_ThrowsInvalidOperationException()
    {
        // Arrange
        var existingEntity = new EntityLifeCycleResult<CurrencyRateEntity, BaseRevisionEntity>
        {
            Entity = _currencyRate,
            Revision = new BaseRevisionEntity { Action = RevisionAction.Created }
        };

        _mocker.GetMock<IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity>>()
            .Setup(h => h.FindAsync(
                It.IsAny<Expression<Func<CurrencyRateEntity, bool>>>(),
                It.IsAny<Expression<Func<BaseRevisionEntity, bool>>>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { existingEntity });

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CreateAsync(_currencyRate, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_ExistingDeletedEntity_RestoresEntity()
    {
        // Arrange
        var deletedEntity = new EntityLifeCycleResult<CurrencyRateEntity, BaseRevisionEntity>
        {
            Entity = _currencyRate,
            Revision = new BaseRevisionEntity { Action = RevisionAction.Deleted, Key = Guid.NewGuid() }
        };

        var restoredEntity = new EntityLifeCycleResult<CurrencyRateEntity, BaseRevisionEntity>
        {
            Entity = _currencyRate,
            Revision = new BaseRevisionEntity { Action = RevisionAction.Created, Key = deletedEntity.Key }
        };

        _mocker.GetMock<IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity>>()
            .Setup(h => h.FindAsync(
                It.IsAny<Expression<Func<CurrencyRateEntity, bool>>>(),
                It.IsAny<Expression<Func<BaseRevisionEntity, bool>>>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { deletedEntity });

        _mocker.GetMock<IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity>>()
            .Setup(h => h.RestoreAsync(deletedEntity.Key, It.IsAny<BaseRevisionEntity?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(restoredEntity);

        // Act
        var result = await _sut.CreateAsync(_currencyRate);

        // Assert
        _mocker.GetMock<IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity>>()
            .Verify(h => h.RestoreAsync(deletedEntity.Key, It.IsAny<BaseRevisionEntity?>(), CancellationToken.None), Times.Once);
        Assert.Equal(ManagerResponseState.Success, result.State);
        Assert.Equal(restoredEntity, result.CurrencyRate);
    }

    [Fact]
    public async Task CreateAsync_ChecksForExistingEntityWithCorrectFilters()
    {
        // Act
        await _sut.CreateAsync(_currencyRate);

        // Assert
        _mocker.GetMock<IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity>>()
            .Verify(h => h.FindAsync(
                It.Is<Expression<Func<CurrencyRateEntity, bool>>>(f =>
                    f.Body.ToString()!.Contains(nameof(CurrencyRateEntity.OriginKey)) &&
                    f.Body.ToString()!.Contains(nameof(CurrencyRateEntity.TargetKey))),
                It.Is<Expression<Func<BaseRevisionEntity, bool>>>(f =>
                    f.Body.ToString()!.Contains(nameof(BaseRevisionEntity.Status))),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()),
                Times.Once);
    }
}