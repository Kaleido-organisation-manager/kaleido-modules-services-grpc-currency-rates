using System.Linq.Expressions;
using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Constants;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Models;
using Kaleido.Modules.Services.Grpc.CurrencyRates.GetAllConversions;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Unit.Builders;
using Moq;
using Moq.AutoMock;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Unit.GetAllConversions;

public class GetAllConversionsManagerTests
{
    private readonly AutoMocker _mocker;
    private readonly GetAllConversionsManager _sut;
    private readonly Guid _validKey;
    private readonly List<EntityLifeCycleResult<CurrencyRateEntity, BaseRevisionEntity>> _entityResults;

    public GetAllConversionsManagerTests()
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
                    Action = RevisionAction.Created,
                    Status = RevisionStatus.Active
                }
            }
        };

        _mocker.GetMock<IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity>>()
            .Setup(h => h.FindAllAsync(
                It.IsAny<Expression<Func<CurrencyRateEntity, bool>>>(),
                It.IsAny<Expression<Func<BaseRevisionEntity, bool>>>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(_entityResults);

        _sut = _mocker.CreateInstance<GetAllConversionsManager>();
    }

    [Fact]
    public async Task GetAllConversionsAsync_ValidKey_CallsEntityLifecycleHandler()
    {
        // Act
        await _sut.GetAllConversionsAsync(_validKey);

        // Assert
        _mocker.GetMock<IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity>>()
            .Verify(h => h.FindAllAsync(
                It.IsAny<Expression<Func<CurrencyRateEntity, bool>>>(),
                It.IsAny<Expression<Func<BaseRevisionEntity, bool>>>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllConversionsAsync_ValidKey_ReturnsSuccessResponses()
    {
        // Act
        var results = await _sut.GetAllConversionsAsync(_validKey);

        // Assert
        Assert.NotNull(results);
        Assert.NotEmpty(results);
        Assert.All(results, r => Assert.Equal(ManagerResponseState.Success, r.State));
        Assert.All(results, r => Assert.NotNull(r.CurrencyRate));
    }

    [Fact]
    public async Task GetAllConversionsAsync_NoConversionsFound_ReturnsEmptyList()
    {
        // Arrange
        _mocker.GetMock<IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity>>()
            .Setup(h => h.FindAllAsync(
                It.IsAny<Expression<Func<CurrencyRateEntity, bool>>>(),
                It.IsAny<Expression<Func<BaseRevisionEntity, bool>>>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EntityLifeCycleResult<CurrencyRateEntity, BaseRevisionEntity>>());

        // Act
        var results = await _sut.GetAllConversionsAsync(_validKey);

        // Assert
        Assert.NotNull(results);
        Assert.Empty(results);
    }

    [Fact]
    public async Task GetAllConversionsAsync_CorrectlyFiltersRevisions()
    {
        // Act
        await _sut.GetAllConversionsAsync(_validKey);

        // Assert
        _mocker.GetMock<IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity>>()
            .Verify(h => h.FindAllAsync(
                It.Is<Expression<Func<CurrencyRateEntity, bool>>>(f => f.Body.ToString()!.Contains(nameof(CurrencyRateEntity.OriginKey))),
                It.Is<Expression<Func<BaseRevisionEntity, bool>>>(f =>
                    f.Body.ToString()!.Contains(nameof(BaseRevisionEntity.Status)) &&
                    f.Body.ToString()!.Contains(nameof(BaseRevisionEntity.Action))),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()), Times.Once);
    }
}