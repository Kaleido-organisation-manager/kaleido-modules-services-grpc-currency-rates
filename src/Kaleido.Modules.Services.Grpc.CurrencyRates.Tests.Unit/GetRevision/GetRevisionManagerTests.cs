using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Exceptions;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Constants;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Models;
using Kaleido.Modules.Services.Grpc.CurrencyRates.GetRevision;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Unit.Builders;
using Moq;
using Moq.AutoMock;
using Xunit;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Unit.GetRevision;

public class GetRevisionManagerTests
{
    private readonly AutoMocker _mocker;
    private readonly GetRevisionManager _sut;
    private readonly Guid _validKey;
    private readonly DateTime _validCreatedAt;
    private readonly EntityLifeCycleResult<CurrencyRateEntity, BaseRevisionEntity> _entityResult;

    public GetRevisionManagerTests()
    {
        _mocker = new AutoMocker();
        _validKey = Guid.NewGuid();
        _validCreatedAt = DateTime.UtcNow;

        var entity = new CurrencyRateBuilder().BuildEntity();
        _entityResult = new EntityLifeCycleResult<CurrencyRateEntity, BaseRevisionEntity>
        {
            Entity = entity,
            Revision = new BaseRevisionEntity
            {
                Action = RevisionAction.Created,
                Status = RevisionStatus.Active
            }
        };

        _mocker.GetMock<IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity>>()
            .Setup(h => h.GetHistoricAsync(_validKey, _validCreatedAt, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_entityResult);

        _sut = _mocker.CreateInstance<GetRevisionManager>();
    }

    [Fact]
    public async Task GetRevisionAsync_ValidRequest_CallsEntityLifecycleHandler()
    {
        // Act
        await _sut.GetRevisionAsync(_validKey, _validCreatedAt, CancellationToken.None);

        // Assert
        _mocker.GetMock<IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity>>()
            .Verify(h => h.GetHistoricAsync(_validKey, _validCreatedAt, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetRevisionAsync_ValidRequest_ReturnsSuccessResponse()
    {
        // Act
        var result = await _sut.GetRevisionAsync(_validKey, _validCreatedAt, CancellationToken.None);

        // Assert
        Assert.Equal(ManagerResponseState.Success, result.State);
        Assert.Equal(_entityResult, result.CurrencyRate);
    }

    [Fact]
    public async Task GetRevisionAsync_EntityNotFound_ReturnsNotFoundResponse()
    {
        // Arrange
        _mocker.GetMock<IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity>>()
            .Setup(h => h.GetHistoricAsync(_validKey, _validCreatedAt, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException("Entity not found"));

        // Act
        var result = await _sut.GetRevisionAsync(_validKey, _validCreatedAt, CancellationToken.None);

        // Assert
        Assert.Equal(ManagerResponseState.NotFound, result.State);
    }

    [Fact]
    public async Task GetRevisionAsync_RevisionNotFound_ReturnsNotFoundResponse()
    {
        // Arrange
        _mocker.GetMock<IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity>>()
            .Setup(h => h.GetHistoricAsync(_validKey, _validCreatedAt, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RevisionNotFoundException("Revision not found"));

        // Act
        var result = await _sut.GetRevisionAsync(_validKey, _validCreatedAt, CancellationToken.None);

        // Assert
        Assert.Equal(ManagerResponseState.NotFound, result.State);
    }

    [Fact]
    public async Task GetRevisionAsync_NoRevisionFound_ReturnsNotFoundResponse()
    {
        // Arrange
        _mocker.GetMock<IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity>>()
            .Setup(h => h.GetHistoricAsync(_validKey, _validCreatedAt, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EntityLifeCycleResult<CurrencyRateEntity, BaseRevisionEntity>?)null);

        // Act
        var result = await _sut.GetRevisionAsync(_validKey, _validCreatedAt, CancellationToken.None);

        // Assert
        Assert.Equal(ManagerResponseState.NotFound, result.State);
    }
}