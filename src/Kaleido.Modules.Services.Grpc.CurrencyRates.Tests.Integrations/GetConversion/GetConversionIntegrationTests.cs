using System.Text.Json;
using Grpc.Core;
using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Integrations.Fixtures;
using Xunit;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Integrations.GetConversion;

[Collection(nameof(InfrastructureCollection))]
public class GetConversionIntegrationTests : IAsyncLifetime
{
    private readonly InfrastructureFixture _fixture;

    public GetConversionIntegrationTests(InfrastructureFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _fixture.ClearDatabase();

    [Fact]
    public async Task GetConversion_ValidKeys_ReturnsConversion()
    {
        // Arrange
        var originKey = Guid.NewGuid();
        var targetKey = Guid.NewGuid();
        var rate = 1.5m;

        var created = await _fixture.Client.CreateAsync(originKey, targetKey, rate);

        // Act
        var result = await _fixture.Client.GetConversionAsync(originKey, targetKey);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(created.Key, result.Key);
        Assert.Equal(originKey, result.Entity.OriginKey);
        Assert.Equal(targetKey, result.Entity.TargetKey);
        Assert.Equal(rate, result.Entity.Rate);
        Assert.Equal(RevisionAction.Created, result.Revision.Action);
        Assert.Equal(RevisionStatus.Active, result.Revision.Status);
    }

    [Fact]
    public async Task GetConversion_NonExistentConversion_ThrowsException()
    {
        // Arrange
        var originKey = Guid.NewGuid();
        var targetKey = Guid.NewGuid();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            () => _fixture.Client.GetConversionAsync(originKey, targetKey));
        Assert.Equal(StatusCode.NotFound, exception.StatusCode);
    }

    [Fact]
    public async Task GetConversion_DeletedConversion_ThrowsException()
    {
        // Arrange
        var originKey = Guid.NewGuid();
        var targetKey = Guid.NewGuid();
        var rate = 1.5m;

        var created = await _fixture.Client.CreateAsync(originKey, targetKey, rate);
        await _fixture.Client.DeleteAsync(created.Key);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            () => _fixture.Client.GetConversionAsync(originKey, targetKey));
        Assert.Equal(StatusCode.NotFound, exception.StatusCode);
    }

    [Fact]
    public async Task GetConversion_UpdatedConversion_ReturnsLatestVersion()
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
        Assert.NotNull(result);
        Assert.Equal(created.Key, result.Key);
        Assert.Equal(originKey, result.Entity.OriginKey);
        Assert.Equal(targetKey, result.Entity.TargetKey);
        Assert.Equal(updatedRate, result.Entity.Rate);
        Assert.Equal(RevisionAction.Updated, result.Revision.Action);
    }

    [Fact]
    public async Task GetConversion_ReverseConversion_ReturnsCorrectRate()
    {
        // Arrange
        var originKey = Guid.NewGuid();
        var targetKey = Guid.NewGuid();
        var forwardRate = 1.5m;
        var reverseRate = Math.Round(1 / forwardRate, 2, MidpointRounding.AwayFromZero);

        await _fixture.Client.CreateAsync(originKey, targetKey, forwardRate);
        var reverseCreated = await _fixture.Client.CreateAsync(targetKey, originKey, reverseRate);

        // Act
        var result = await _fixture.Client.GetConversionAsync(targetKey, originKey);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(reverseCreated.Key, result.Key);
        Assert.Equal(targetKey, result.Entity.OriginKey);
        Assert.Equal(originKey, result.Entity.TargetKey);
        Assert.Equal(reverseRate, result.Entity.Rate);
    }

    [Fact]
    public async Task GetConversion_DeletedAndRestored_ReturnsRestoredConversion()
    {
        // Arrange
        var originKey = Guid.NewGuid();
        var targetKey = Guid.NewGuid();
        var rate = 1.5m;

        var created = await _fixture.Client.CreateAsync(originKey, targetKey, rate);
        await _fixture.Client.DeleteAsync(created.Key);
        var restored = await _fixture.Client.CreateAsync(originKey, targetKey, rate);

        // Act
        var result = await _fixture.Client.GetConversionAsync(originKey, targetKey);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(restored.Key, result.Key);
        Assert.Equal(originKey, result.Entity.OriginKey);
        Assert.Equal(targetKey, result.Entity.TargetKey);
        Assert.Equal(rate, result.Entity.Rate);
        Assert.Equal(RevisionAction.Restored, result.Revision.Action);
    }

    [Fact]
    public async Task GetConversion_MultipleConversions_ReturnsCorrectOne()
    {
        // Arrange
        var originKey = Guid.NewGuid();
        var targetKey1 = Guid.NewGuid();
        var targetKey2 = Guid.NewGuid();
        var rate1 = 1.5m;
        var rate2 = 2.0m;

        await _fixture.Client.CreateAsync(originKey, targetKey1, rate1);
        var created2 = await _fixture.Client.CreateAsync(originKey, targetKey2, rate2);

        // Act
        var result = await _fixture.Client.GetConversionAsync(originKey, targetKey2);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(created2.Key, result.Key);
        Assert.Equal(originKey, result.Entity.OriginKey);
        Assert.Equal(targetKey2, result.Entity.TargetKey);
        Assert.Equal(rate2, result.Entity.Rate);
    }
}