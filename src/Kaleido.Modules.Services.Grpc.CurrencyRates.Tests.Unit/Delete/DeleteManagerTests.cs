using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Exceptions;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Constants;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Models;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Delete;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Unit.Builders;
using Moq;
using Moq.AutoMock;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Unit.Delete;

public class DeleteManagerTests
{
    private readonly AutoMocker _mocker;
    private readonly DeleteManager _sut;
    private readonly Guid _validKey;
    private readonly EntityLifeCycleResult<CurrencyRateEntity, BaseRevisionEntity> _entityResult;

    public DeleteManagerTests()
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
                Action = RevisionAction.Deleted
            }
        };

        var storedEntity = new EntityLifeCycleResult<CurrencyRateEntity, BaseRevisionEntity>
        {
            Entity = entity,
            Revision = new BaseRevisionEntity
            {
                Key = _validKey,
                Action = RevisionAction.Created
            }
        };

        _mocker.GetMock<IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity>>()
            .Setup(h => h.GetAsync(It.IsAny<Guid>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedEntity);

        _mocker.GetMock<IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity>>()
            .Setup(h => h.DeleteAsync(It.IsAny<Guid>(), It.IsAny<BaseRevisionEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_entityResult);

        _sut = _mocker.CreateInstance<DeleteManager>();
    }

    [Fact]
    public async Task DeleteAsync_ValidKey_CallsEntityLifecycleHandler()
    {
        // Act
        await _sut.DeleteAsync(_validKey, CancellationToken.None);

        // Assert
        _mocker.GetMock<IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity>>()
            .Verify(h => h.DeleteAsync(_validKey, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ValidKey_ReturnsSuccessResponse()
    {
        // Act
        var result = await _sut.DeleteAsync(_validKey, CancellationToken.None);

        // Assert
        Assert.Equal(ManagerResponseState.Success, result.State);
        Assert.Equal(_entityResult, result.CurrencyRate);
    }

    [Fact]
    public async Task DeleteAsync_EntityNotFound_ReturnsNotFoundResponse()
    {
        // Arrange
        _mocker.GetMock<IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity>>()
            .Setup(h => h.GetAsync(It.IsAny<Guid>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException("Entity not found"));

        // Act
        var result = await _sut.DeleteAsync(_validKey, CancellationToken.None);

        // Assert
        Assert.Equal(ManagerResponseState.NotFound, result.State);
    }

    [Fact]
    public async Task DeleteAsync_RevisionNotFound_ReturnsNotFoundResponse()
    {
        // Arrange
        _mocker.GetMock<IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity>>()
            .Setup(h => h.GetAsync(It.IsAny<Guid>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RevisionNotFoundException("Revision not found"));

        // Act
        var result = await _sut.DeleteAsync(_validKey, CancellationToken.None);

        // Assert
        Assert.Equal(ManagerResponseState.NotFound, result.State);
    }

    [Fact]
    public async Task DeleteAsync_EntityAlreadyDeleted_ReturnsNotFoundResponse()
    {
        // Arrange
        var deletedResult = new EntityLifeCycleResult<CurrencyRateEntity, BaseRevisionEntity>
        {
            Entity = _entityResult.Entity,
            Revision = new BaseRevisionEntity { Action = RevisionAction.Deleted }
        };

        _mocker.GetMock<IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity>>()
            .Setup(h => h.GetAsync(It.IsAny<Guid>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(deletedResult);

        // Act
        var result = await _sut.DeleteAsync(_validKey, CancellationToken.None);

        // Assert
        Assert.Equal(ManagerResponseState.NotFound, result.State);
    }
}