using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Integrations.Fixtures;
using Xunit;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Integrations.GetAll;

[Collection(nameof(InfrastructureCollection))]
public class GetAllIntegrationTests : IAsyncLifetime
{
    private readonly InfrastructureFixture _fixture;

    public GetAllIntegrationTests(InfrastructureFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _fixture.ClearDatabase();

    [Fact]
    public async Task GetAllCurrencyRates_EmptyDatabase_ReturnsEmptyList()
    {
        // Act
        var result = await _fixture.Client.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllCurrencyRates_SingleRate_ReturnsOneItem()
    {
        // Arrange
        var originKey = Guid.NewGuid();
        var targetKey = Guid.NewGuid();
        var rate = 1.5m;

        var created = await _fixture.Client.CreateAsync(originKey, targetKey, rate);

        // Act
        var result = await _fixture.Client.GetAllAsync();

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
    public async Task GetAllCurrencyRates_MultipleRates_ReturnsAllItems()
    {
        // Arrange
        var rates = new[]
        {
            (originKey: Guid.NewGuid(), targetKey: Guid.NewGuid(), rate: 1.5m),
            (originKey: Guid.NewGuid(), targetKey: Guid.NewGuid(), rate: 2.0m),
            (originKey: Guid.NewGuid(), targetKey: Guid.NewGuid(), rate: 0.75m)
        };

        foreach (var (originKey, targetKey, rate) in rates)
        {
            await _fixture.Client.CreateAsync(originKey, targetKey, rate);
        }

        // Act
        var result = await _fixture.Client.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(rates.Length, result.Count());
        foreach (var (originKey, targetKey, rate) in rates)
        {
            Assert.Contains(result, r =>
                r.Entity.OriginKey == originKey &&
                r.Entity.TargetKey == targetKey &&
                r.Entity.Rate == rate);
        }
    }

    [Fact]
    public async Task GetAllCurrencyRates_DeletedRate_NotIncludedInResults()
    {
        // Arrange
        var originKey1 = Guid.NewGuid();
        var targetKey1 = Guid.NewGuid();
        var rate1 = 1.5m;

        var originKey2 = Guid.NewGuid();
        var targetKey2 = Guid.NewGuid();
        var rate2 = 2.0m;

        var created1 = await _fixture.Client.CreateAsync(originKey1, targetKey1, rate1);
        var created2 = await _fixture.Client.CreateAsync(originKey2, targetKey2, rate2);
        await _fixture.Client.DeleteAsync(created1.Key);

        // Act
        var result = await _fixture.Client.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        var item = result.First();
        Assert.Equal(created2.Key, item.Key);
        Assert.Equal(originKey2, item.Entity.OriginKey);
        Assert.Equal(targetKey2, item.Entity.TargetKey);
        Assert.Equal(rate2, item.Entity.Rate);
    }

    [Fact]
    public async Task GetAllCurrencyRates_UpdatedRate_ReturnsLatestVersion()
    {
        // Arrange
        var originKey = Guid.NewGuid();
        var targetKey = Guid.NewGuid();
        var initialRate = 1.5m;
        var updatedRate = 2.0m;

        var created = await _fixture.Client.CreateAsync(originKey, targetKey, initialRate);
        await _fixture.Client.UpdateAsync(created.Key, updatedRate);

        // Act
        var result = await _fixture.Client.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        var item = result.First();
        Assert.Equal(created.Key, item.Key);
        Assert.Equal(originKey, item.Entity.OriginKey);
        Assert.Equal(targetKey, item.Entity.TargetKey);
        Assert.Equal(updatedRate, item.Entity.Rate);
        Assert.Equal(RevisionAction.Updated, item.Revision.Action);
    }

    [Fact]
    public async Task GetAllCurrencyRates_DeletedAndRestored_IncludedInResults()
    {
        // Arrange
        var originKey = Guid.NewGuid();
        var targetKey = Guid.NewGuid();
        var rate = 1.5m;

        var created = await _fixture.Client.CreateAsync(originKey, targetKey, rate);
        await _fixture.Client.DeleteAsync(created.Key);
        var restored = await _fixture.Client.CreateAsync(originKey, targetKey, rate);

        // Act
        var result = await _fixture.Client.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        var item = result.First();
        Assert.Equal(restored.Key, item.Key);
        Assert.Equal(originKey, item.Entity.OriginKey);
        Assert.Equal(targetKey, item.Entity.TargetKey);
        Assert.Equal(rate, item.Entity.Rate);
        Assert.Equal(RevisionAction.Restored, item.Revision.Action);
    }
}