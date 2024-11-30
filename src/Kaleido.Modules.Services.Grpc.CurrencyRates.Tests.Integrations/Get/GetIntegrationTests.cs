using Grpc.Core;
using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Integrations.Fixtures;
using Xunit;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Integrations.Get;

[Collection(nameof(InfrastructureCollection))]
public class GetIntegrationTests : IAsyncLifetime
{
    private readonly InfrastructureFixture _fixture;

    public GetIntegrationTests(InfrastructureFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _fixture.ClearDatabase();

    [Fact]
    public async Task GetCurrencyRate_ValidKey_ReturnsRate()
    {
        // Arrange
        var originKey = Guid.NewGuid();
        var targetKey = Guid.NewGuid();
        var rate = 1.5m;

        var created = await _fixture.Client.CreateAsync(originKey, targetKey, rate);

        // Act
        var result = await _fixture.Client.GetAsync(created.Key);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(created.Key, result.Key);
        Assert.Equal(originKey, result.Entity.OriginKey);
        Assert.Equal(targetKey, result.Entity.TargetKey);
        Assert.Equal(rate, result.Entity.Rate);
        Assert.NotNull(result.Revision);
        Assert.Equal(RevisionAction.Created, result.Revision.Action);
        Assert.Equal(RevisionStatus.Active, result.Revision.Status);
    }

    [Fact]
    public async Task GetCurrencyRate_NonExistentKey_ThrowsException()
    {
        // Arrange
        var nonExistentKey = Guid.NewGuid();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            () => _fixture.Client.GetAsync(nonExistentKey));
        Assert.Equal(StatusCode.NotFound, exception.StatusCode);
    }

    [Fact]
    public async Task GetCurrencyRate_DeletedRate_ThrowsException()
    {
        // Arrange
        var originKey = Guid.NewGuid();
        var targetKey = Guid.NewGuid();
        var rate = 1.5m;

        var created = await _fixture.Client.CreateAsync(originKey, targetKey, rate);
        await _fixture.Client.DeleteAsync(created.Key);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            () => _fixture.Client.GetAsync(created.Key));
        Assert.Equal(StatusCode.NotFound, exception.StatusCode);
    }


    [Fact]
    public async Task GetCurrencyRate_MultipleRates_ReturnsCorrectRate()
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

        // Act
        var result1 = await _fixture.Client.GetAsync(created1.Key);
        var result2 = await _fixture.Client.GetAsync(created2.Key);

        // Assert
        Assert.NotEqual(result1.Key, result2.Key);
        Assert.Equal(rate1, result1.Entity.Rate);
        Assert.Equal(rate2, result2.Entity.Rate);
    }

    [Fact]
    public async Task GetCurrencyRate_UpdatedRate_ReturnsLatestVersion()
    {
        // Arrange
        var originKey = Guid.NewGuid();
        var targetKey = Guid.NewGuid();
        var initialRate = 1.5m;
        var updatedRate = 2.0m;

        var created = await _fixture.Client.CreateAsync(originKey, targetKey, initialRate);
        await _fixture.Client.UpdateAsync(created.Key, updatedRate);

        // Act
        var result = await _fixture.Client.GetAsync(created.Key);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(created.Key, result.Key);
        Assert.Equal(updatedRate, result.Entity.Rate);
        Assert.Equal(RevisionAction.Updated, result.Revision.Action);
    }
}