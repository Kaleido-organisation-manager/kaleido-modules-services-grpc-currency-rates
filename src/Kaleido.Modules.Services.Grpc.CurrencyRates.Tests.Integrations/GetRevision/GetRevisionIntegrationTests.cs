using Grpc.Core;
using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Integrations.Fixtures;
using Xunit;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Integrations.GetRevision;

[Collection(nameof(InfrastructureCollection))]
public class GetRevisionIntegrationTests : IAsyncLifetime
{
    private readonly InfrastructureFixture _fixture;

    public GetRevisionIntegrationTests(InfrastructureFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _fixture.ClearDatabase();

    [Fact]
    public async Task GetRevision_CreatedRevision_ReturnsCorrectRevision()
    {
        // Arrange
        var originKey = Guid.NewGuid();
        var targetKey = Guid.NewGuid();
        var rate = 1.5m;

        var created = await _fixture.Client.CreateAsync(originKey, targetKey, rate);
        var createdAt = created.Revision.CreatedAt;

        // Act
        var result = await _fixture.Client.GetRevisionAsync(created.Key, createdAt);

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
    public async Task GetRevision_UpdatedRevision_ReturnsCorrectRevision()
    {
        // Arrange
        var originKey = Guid.NewGuid();
        var targetKey = Guid.NewGuid();
        var initialRate = 1.5m;
        var updatedRate = 2.0m;

        var created = await _fixture.Client.CreateAsync(originKey, targetKey, initialRate);
        var updated = await _fixture.Client.UpdateAsync(created.Key, updatedRate);
        var updatedAt = updated.Revision.CreatedAt;

        // Act
        var result = await _fixture.Client.GetRevisionAsync(created.Key, updatedAt);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(created.Key, result.Key);
        Assert.Equal(originKey, result.Entity.OriginKey);
        Assert.Equal(targetKey, result.Entity.TargetKey);
        Assert.Equal(updatedRate, result.Entity.Rate);
        Assert.Equal(RevisionAction.Updated, result.Revision.Action);
    }

    [Fact]
    public async Task GetRevision_DeletedRevision_ReturnsCorrectRevision()
    {
        // Arrange
        var originKey = Guid.NewGuid();
        var targetKey = Guid.NewGuid();
        var rate = 1.5m;

        var created = await _fixture.Client.CreateAsync(originKey, targetKey, rate);
        var deleted = await _fixture.Client.DeleteAsync(created.Key);
        var deletedAt = deleted.Revision.CreatedAt;

        // Act
        var result = await _fixture.Client.GetRevisionAsync(created.Key, deletedAt);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(created.Key, result.Key);
        Assert.Equal(originKey, result.Entity.OriginKey);
        Assert.Equal(targetKey, result.Entity.TargetKey);
        Assert.Equal(rate, result.Entity.Rate);
        Assert.Equal(RevisionAction.Deleted, result.Revision.Action);
    }

    [Fact]
    public async Task GetRevision_NonExistentKey_ThrowsException()
    {
        // Arrange
        var nonExistentKey = Guid.NewGuid();
        var timestamp = DateTime.UtcNow;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            () => _fixture.Client.GetRevisionAsync(nonExistentKey, timestamp));
        Assert.Equal(StatusCode.NotFound, exception.StatusCode);
    }

    [Fact]
    public async Task GetRevision_NonExistentTimestamp_ThrowsException()
    {
        // Arrange
        var originKey = Guid.NewGuid();
        var targetKey = Guid.NewGuid();
        var rate = 1.5m;

        var created = await _fixture.Client.CreateAsync(originKey, targetKey, rate);
        var nonExistentTimestamp = created.Revision.CreatedAt.AddDays(-1);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            () => _fixture.Client.GetRevisionAsync(created.Key, nonExistentTimestamp));
        Assert.Equal(StatusCode.NotFound, exception.StatusCode);
    }

    [Fact]
    public async Task GetRevision_MultipleRevisions_ReturnsCorrectRevision()
    {
        // Arrange
        var originKey = Guid.NewGuid();
        var targetKey = Guid.NewGuid();
        var initialRate = 1.5m;
        var updatedRate = 2.0m;

        var created = await _fixture.Client.CreateAsync(originKey, targetKey, initialRate);
        await Task.Delay(100); // Ensure different timestamps
        var updated = await _fixture.Client.UpdateAsync(created.Key, updatedRate);
        await Task.Delay(100); // Ensure different timestamps
        var deleted = await _fixture.Client.DeleteAsync(created.Key);

        // Get and verify each revision
        var createdRevision = await _fixture.Client.GetRevisionAsync(created.Key, created.Revision.CreatedAt);
        var updatedRevision = await _fixture.Client.GetRevisionAsync(created.Key, updated.Revision.CreatedAt);
        var deletedRevision = await _fixture.Client.GetRevisionAsync(created.Key, deleted.Revision.CreatedAt);

        // Assert
        Assert.Equal(initialRate, createdRevision.Entity.Rate);
        Assert.Equal(RevisionAction.Created, createdRevision.Revision.Action);

        Assert.Equal(updatedRate, updatedRevision.Entity.Rate);
        Assert.Equal(RevisionAction.Updated, updatedRevision.Revision.Action);

        Assert.Equal(updatedRate, deletedRevision.Entity.Rate);
        Assert.Equal(RevisionAction.Deleted, deletedRevision.Revision.Action);
    }

    [Fact]
    public async Task GetRevision_RestoredEntity_ReturnsCorrectRevision()
    {
        // Arrange
        var originKey = Guid.NewGuid();
        var targetKey = Guid.NewGuid();
        var rate = 1.5m;

        var created = await _fixture.Client.CreateAsync(originKey, targetKey, rate);
        var deleted = await _fixture.Client.DeleteAsync(created.Key);
        var restored = await _fixture.Client.CreateAsync(originKey, targetKey, rate);

        // Act
        var result = await _fixture.Client.GetRevisionAsync(restored.Key, restored.Revision.CreatedAt);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(restored.Key, result.Key);
        Assert.Equal(rate, result.Entity.Rate);
        Assert.Equal(RevisionAction.Restored, result.Revision.Action);
    }
}