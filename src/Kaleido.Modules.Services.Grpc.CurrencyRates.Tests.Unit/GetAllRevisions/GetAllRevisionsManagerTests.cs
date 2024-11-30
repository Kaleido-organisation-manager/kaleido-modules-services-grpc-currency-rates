using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Constants;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Models;
using Kaleido.Modules.Services.Grpc.CurrencyRates.GetAllRevisions;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Unit.Builders;
using Moq;
using Moq.AutoMock;
using Xunit;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Unit.GetAllRevisions;

public class GetAllRevisionsManagerTests
{
    private readonly AutoMocker _mocker;
    private readonly GetAllRevisionsManager _sut;
    private readonly Guid _validKey;
    private readonly List<EntityLifeCycleResult<CurrencyRateEntity, BaseRevisionEntity>> _entityResults;

    public GetAllRevisionsManagerTests()
    {
        _mocker = new AutoMocker();
        _validKey = Guid.NewGuid();

        var entity = new CurrencyRateBuilder().BuildEntity();
        _entityResults = new List<EntityLifeCycleResult<CurrencyRateEntity, BaseRevisionEntity>>
        {
            new()
            {
                Entity = entity,
                Revision = new BaseRevisionEntity
                {
                    Key = _validKey,
                    Action = RevisionAction.Created
                }
            }
        };

        _mocker.GetMock<IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity>>()
            .Setup(h => h.GetAllAsync(_validKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_entityResults);

        _sut = _mocker.CreateInstance<GetAllRevisionsManager>();
    }

    [Fact]
    public async Task GetAllRevisionsAsync_ValidKey_CallsEntityLifecycleHandler()
    {
        // Act
        await _sut.GetAllRevisionsAsync(_validKey, CancellationToken.None);

        // Assert
        _mocker.GetMock<IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity>>()
            .Verify(h => h.GetAllAsync(_validKey, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllRevisionsAsync_ValidKey_ReturnsSuccessResponses()
    {
        // Act
        var results = await _sut.GetAllRevisionsAsync(_validKey, CancellationToken.None);

        // Assert
        Assert.NotNull(results);
        Assert.NotEmpty(results);
        Assert.All(results, r => Assert.Equal(ManagerResponseState.Success, r.State));
        Assert.All(results, r => Assert.NotNull(r.CurrencyRate));
    }

    [Fact]
    public async Task GetAllRevisionsAsync_NoRevisionsFound_ReturnsEmptyList()
    {
        // Arrange
        _mocker.GetMock<IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity>>()
            .Setup(h => h.GetAllAsync(_validKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EntityLifeCycleResult<CurrencyRateEntity, BaseRevisionEntity>>());

        // Act
        var results = await _sut.GetAllRevisionsAsync(_validKey, CancellationToken.None);

        // Assert
        Assert.NotNull(results);
        Assert.Empty(results);
    }

    [Fact]
    public async Task GetAllRevisionsAsync_ReturnsAllRevisions()
    {
        // Arrange
        var entity = new CurrencyRateBuilder().BuildEntity();
        var multipleResults = new List<EntityLifeCycleResult<CurrencyRateEntity, BaseRevisionEntity>>
        {
            new()
            {
                Entity = entity,
                Revision = new BaseRevisionEntity { Action = RevisionAction.Created }
            },
            new()
            {
                Entity = entity,
                Revision = new BaseRevisionEntity { Action = RevisionAction.Updated }
            },
            new()
            {
                Entity = entity,
                Revision = new BaseRevisionEntity { Action = RevisionAction.Deleted }
            }
        };

        _mocker.GetMock<IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity>>()
            .Setup(h => h.GetAllAsync(_validKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(multipleResults);

        // Act
        var results = await _sut.GetAllRevisionsAsync(_validKey, CancellationToken.None);

        // Assert
        Assert.Equal(multipleResults.Count, results.Count());
        Assert.All(results, r => Assert.Equal(ManagerResponseState.Success, r.State));
    }
}