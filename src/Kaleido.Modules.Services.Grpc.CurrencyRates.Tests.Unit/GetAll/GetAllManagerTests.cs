using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Constants;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Models;
using Kaleido.Modules.Services.Grpc.CurrencyRates.GetAll;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Unit.Builders;
using Moq;
using Moq.AutoMock;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Unit.GetAll;

public class GetAllManagerTests
{
    private readonly AutoMocker _mocker;
    private readonly GetAllManager _sut;
    private readonly List<EntityLifeCycleResult<CurrencyRateEntity, BaseRevisionEntity>> _entityResults;

    public GetAllManagerTests()
    {
        _mocker = new AutoMocker();

        var entity = new CurrencyRateBuilder().BuildEntity();
        _entityResults = new List<EntityLifeCycleResult<CurrencyRateEntity, BaseRevisionEntity>>
        {
            new()
            {
                Entity = entity,
                Revision = new BaseRevisionEntity
                {
                    Action = RevisionAction.Created,
                    Status = RevisionStatus.Active
                }
            }
        };

        _mocker.GetMock<IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity>>()
            .Setup(h => h.GetAllAsync(It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_entityResults);

        _sut = _mocker.CreateInstance<GetAllManager>();
    }

    [Fact]
    public async Task GetAllAsync_CallsEntityLifecycleHandler()
    {
        // Act
        await _sut.GetAllAsync(CancellationToken.None);

        // Assert
        _mocker.GetMock<IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity>>()
            .Verify(h => h.GetAllAsync(It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsSuccessResponses()
    {
        // Act
        var results = await _sut.GetAllAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(results);
        Assert.NotEmpty(results);
        Assert.All(results, r => Assert.Equal(ManagerResponseState.Success, r.State));
        Assert.All(results, r => Assert.NotNull(r.CurrencyRate));
    }

    [Fact]
    public async Task GetAllAsync_NoResults_ReturnsEmptyList()
    {
        // Arrange
        _mocker.GetMock<IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity>>()
            .Setup(h => h.GetAllAsync(It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EntityLifeCycleResult<CurrencyRateEntity, BaseRevisionEntity>>());

        // Act
        var results = await _sut.GetAllAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(results);
        Assert.Empty(results);
    }

    [Fact]
    public async Task GetAllAsync_MapsAllResults()
    {
        // Arrange
        var multipleResults = new List<EntityLifeCycleResult<CurrencyRateEntity, BaseRevisionEntity>>
        {
            new() { Entity = new CurrencyRateBuilder().BuildEntity(), Revision = new BaseRevisionEntity { Action = RevisionAction.Created } },
            new() { Entity = new CurrencyRateBuilder().BuildEntity(), Revision = new BaseRevisionEntity { Action = RevisionAction.Updated } },
            new() { Entity = new CurrencyRateBuilder().BuildEntity(), Revision = new BaseRevisionEntity { Action = RevisionAction.Deleted } }
        };

        _mocker.GetMock<IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity>>()
            .Setup(h => h.GetAllAsync(It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(multipleResults);

        // Act
        var results = await _sut.GetAllAsync(CancellationToken.None);

        // Assert
        Assert.Equal(multipleResults.Count, results.Count());
        Assert.All(results, r => Assert.Equal(ManagerResponseState.Success, r.State));
    }
}