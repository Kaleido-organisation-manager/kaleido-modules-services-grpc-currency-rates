using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Integrations.Fixtures;
using Xunit;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Integrations.GetAllConversions;

[Collection(nameof(InfrastructureCollection))]
public class GetAllConversionsIntegrationTests : IAsyncLifetime
{
    private readonly InfrastructureFixture _fixture;

    public GetAllConversionsIntegrationTests(InfrastructureFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _fixture.ClearDatabase();

    [Fact]
    public async Task GetAllConversions_EmptyDatabase_ReturnsEmptyList()
    {
        // Arrange
        var currencyKey = Guid.NewGuid();

        // Act
        var result = await _fixture.Client.GetAllConversionsAsync(currencyKey);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllConversions_SingleConversion_ReturnsOneItem()
    {
        // Arrange
        var originKey = Guid.NewGuid();
        var targetKey = Guid.NewGuid();
        var rate = 1.5m;

        var created = await _fixture.Client.CreateAsync(originKey, targetKey, rate);

        // Act
        var result = await _fixture.Client.GetAllConversionsAsync(originKey);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        var item = result.First();
        Assert.Equal(created.Key, item.Key);
        Assert.Equal(originKey, item.Entity.OriginKey);
        Assert.Equal(targetKey, item.Entity.TargetKey);
        Assert.Equal(rate, item.Entity.Rate);
        Assert.Equal(RevisionAction.Created, item.Revision.Action);
        Assert.Equal(RevisionStatus.Active, item.Revision.Status);
    }

    [Fact]
    public async Task GetAllConversions_MultipleConversions_ReturnsAllItems()
    {
        // Arrange
        var originKey = Guid.NewGuid();
        var rates = new[]
        {
            (targetKey: Guid.NewGuid(), rate: 1.5m),
            (targetKey: Guid.NewGuid(), rate: 2.0m),
            (targetKey: Guid.NewGuid(), rate: 0.75m)
        };

        foreach (var (targetKey, rate) in rates)
        {
            await _fixture.Client.CreateAsync(originKey, targetKey, rate);
        }

        // Act
        var result = await _fixture.Client.GetAllConversionsAsync(originKey);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(rates.Length, result.Count());
        foreach (var (targetKey, rate) in rates)
        {
            Assert.Contains(result, r =>
                r.Entity.OriginKey == originKey &&
                r.Entity.TargetKey == targetKey &&
                r.Entity.Rate == rate);
        }
    }

    [Fact]
    public async Task GetAllConversions_DeletedConversion_NotIncludedInResults()
    {
        // Arrange
        var originKey = Guid.NewGuid();
        var targetKey1 = Guid.NewGuid();
        var targetKey2 = Guid.NewGuid();
        var rate = 1.5m;

        var created1 = await _fixture.Client.CreateAsync(originKey, targetKey1, rate);
        await _fixture.Client.CreateAsync(originKey, targetKey2, rate);
        await _fixture.Client.DeleteAsync(created1.Key);

        // Act
        var result = await _fixture.Client.GetAllConversionsAsync(originKey);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        var item = result.First();
        Assert.Equal(targetKey2, item.Entity.TargetKey);
    }

    [Fact]
    public async Task GetAllConversions_BidirectionalConversions_ReturnsCorrectItems()
    {
        // Arrange
        var currencyKey1 = Guid.NewGuid();
        var currencyKey2 = Guid.NewGuid();
        var forwardRate = 1.5m;
        var reverseRate = Math.Round(1 / forwardRate, 2, MidpointRounding.AwayFromZero);

        await _fixture.Client.CreateAsync(currencyKey1, currencyKey2, forwardRate);
        await _fixture.Client.CreateAsync(currencyKey2, currencyKey1, reverseRate);

        // Act
        var forwardResults = await _fixture.Client.GetAllConversionsAsync(currencyKey1);
        var reverseResults = await _fixture.Client.GetAllConversionsAsync(currencyKey2);

        // Assert
        Assert.Single(forwardResults);
        Assert.Single(reverseResults);
        Assert.Equal(forwardRate, forwardResults.First().Entity.Rate);
        Assert.Equal(reverseRate, reverseResults.First().Entity.Rate);
    }

    [Fact]
    public async Task GetAllConversions_UpdatedConversion_ReturnsLatestVersion()
    {
        // Arrange
        var originKey = Guid.NewGuid();
        var targetKey = Guid.NewGuid();
        var initialRate = 1.5m;
        var updatedRate = 2.0m;

        var created = await _fixture.Client.CreateAsync(originKey, targetKey, initialRate);
        await _fixture.Client.UpdateAsync(created.Key, updatedRate);

        // Act
        var result = await _fixture.Client.GetAllConversionsAsync(originKey);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        var item = result.First();
        Assert.Equal(created.Key, item.Key);
        Assert.Equal(updatedRate, item.Entity.Rate);
        Assert.Equal(RevisionAction.Updated, item.Revision.Action);
    }

    [Fact]
    public async Task GetAllConversions_NonExistentCurrency_ReturnsEmptyList()
    {
        // Arrange
        var nonExistentKey = Guid.NewGuid();

        // Act
        var result = await _fixture.Client.GetAllConversionsAsync(nonExistentKey);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}