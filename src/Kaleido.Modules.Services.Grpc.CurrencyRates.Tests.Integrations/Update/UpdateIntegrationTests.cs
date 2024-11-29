using Grpc.Core;
using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Integrations.Fixtures;
using Xunit;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Integrations.Update;

[Collection(nameof(InfrastructureCollection))]
public class UpdateIntegrationTests : IAsyncLifetime
{
    private readonly InfrastructureFixture _fixture;

    public UpdateIntegrationTests(InfrastructureFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _fixture.ClearDatabase();

    [Fact]
    public async Task UpdateCurrencyRate_ValidRequest_UpdatesRate()
    {
        // Arrange
        var originKey = Guid.NewGuid();
        var targetKey = Guid.NewGuid();
        var initialRate = 1.5m;
        var updatedRate = 2.0m;

        var created = await _fixture.Client.CreateAsync(originKey, targetKey, initialRate);

        // Act
        var result = await _fixture.Client.UpdateAsync(created.Key, updatedRate);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(created.Key, result.Key);
        Assert.Equal(originKey, result.Entity.OriginKey);
        Assert.Equal(targetKey, result.Entity.TargetKey);
        Assert.Equal(updatedRate, result.Entity.Rate);
        Assert.Equal(RevisionAction.Updated, result.Revision.Action);
        Assert.Equal(RevisionStatus.Active, result.Revision.Status);
    }

    [Fact]
    public async Task UpdateCurrencyRate_NonExistentKey_ThrowsException()
    {
        // Arrange
        var nonExistentKey = Guid.NewGuid();
        var rate = 1.5m;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            () => _fixture.Client.UpdateAsync(nonExistentKey, rate));
        Assert.Equal(StatusCode.NotFound, exception.StatusCode);
    }

    [Fact]
    public async Task UpdateCurrencyRate_DeletedRate_ThrowsException()
    {
        // Arrange
        var originKey = Guid.NewGuid();
        var targetKey = Guid.NewGuid();
        var initialRate = 1.5m;
        var updatedRate = 2.0m;

        var created = await _fixture.Client.CreateAsync(originKey, targetKey, initialRate);
        await _fixture.Client.DeleteAsync(created.Key);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            () => _fixture.Client.UpdateAsync(created.Key, updatedRate));
        Assert.Equal(StatusCode.NotFound, exception.StatusCode);
    }

    [Fact]
    public async Task UpdateCurrencyRate_InvalidRate_ThrowsException()
    {
        // Arrange
        var originKey = Guid.NewGuid();
        var targetKey = Guid.NewGuid();
        var initialRate = 1.5m;
        var invalidRate = -1m;

        var created = await _fixture.Client.CreateAsync(originKey, targetKey, initialRate);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            () => _fixture.Client.UpdateAsync(created.Key, invalidRate));
        Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
    }

    [Fact]
    public async Task UpdateCurrencyRate_SameRate_ThrowsException()
    {
        // Arrange
        var originKey = Guid.NewGuid();
        var targetKey = Guid.NewGuid();
        var rate = 1.5m;

        var created = await _fixture.Client.CreateAsync(originKey, targetKey, rate);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            () => _fixture.Client.UpdateAsync(created.Key, rate));
        Assert.Equal(StatusCode.AlreadyExists, exception.StatusCode);
    }

    [Fact]
    public async Task UpdateCurrencyRate_MultipleUpdates_CreatesNewRevisions()
    {
        // Arrange
        var originKey = Guid.NewGuid();
        var targetKey = Guid.NewGuid();
        var rates = new[] { 1.5m, 2.0m, 2.5m };

        var created = await _fixture.Client.CreateAsync(originKey, targetKey, rates[0]);

        // Act
        var update1 = await _fixture.Client.UpdateAsync(created.Key, rates[1]);
        var update2 = await _fixture.Client.UpdateAsync(created.Key, rates[2]);

        // Assert
        var revisions = await _fixture.Client.GetAllRevisionsAsync(created.Key);
        var revisionsList = revisions.ToList();

        Assert.Equal(3, revisionsList.Count);
        Assert.Contains(revisionsList, r => r.Entity.Rate == rates[0] && r.Revision.Action == RevisionAction.Created);
        Assert.Contains(revisionsList, r => r.Entity.Rate == rates[1] && r.Revision.Action == RevisionAction.Updated);
        Assert.Contains(revisionsList, r => r.Entity.Rate == rates[2] && r.Revision.Action == RevisionAction.Updated);
    }

    [Fact]
    public async Task UpdateCurrencyRate_UpdatedRate_ReflectedInGetAll()
    {
        // Arrange
        var originKey = Guid.NewGuid();
        var targetKey = Guid.NewGuid();
        var initialRate = 1.5m;
        var updatedRate = 2.0m;

        var created = await _fixture.Client.CreateAsync(originKey, targetKey, initialRate);
        await _fixture.Client.UpdateAsync(created.Key, updatedRate);

        // Act
        var allRates = await _fixture.Client.GetAllAsync();

        // Assert
        var updatedItem = Assert.Single(allRates);
        Assert.Equal(updatedRate, updatedItem.Entity.Rate);
        Assert.Equal(RevisionAction.Updated, updatedItem.Revision.Action);
    }

    [Fact]
    public async Task UpdateCurrencyRate_UpdatedRate_ReflectedInGetConversion()
    {
        // Arrange
        var originKey = Guid.NewGuid();
        var targetKey = Guid.NewGuid();
        var initialRate = 1.5m;
        var updatedRate = 2.0m;

        var created = await _fixture.Client.CreateAsync(originKey, targetKey, initialRate);
        await _fixture.Client.UpdateAsync(created.Key, updatedRate);

        // Act
        var result = await _fixture.Client.GetConversionAsync(originKey, targetKey);

        // Assert
        Assert.Equal(updatedRate, result.Entity.Rate);
        Assert.Equal(RevisionAction.Updated, result.Revision.Action);
    }
}