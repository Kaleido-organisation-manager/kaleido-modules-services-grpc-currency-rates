using Grpc.Core;
using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Integrations.Fixtures;
using Xunit;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Integrations.Delete;

[Collection(nameof(InfrastructureCollection))]
public class DeleteIntegrationTests : IAsyncLifetime
{
    private readonly InfrastructureFixture _fixture;

    public DeleteIntegrationTests(InfrastructureFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _fixture.ClearDatabase();

    [Fact]
    public async Task DeleteCurrencyRate_ValidKey_DeletesRate()
    {
        // Arrange
        var originKey = Guid.NewGuid();
        var targetKey = Guid.NewGuid();
        var rate = 1.5m;

        var created = await _fixture.Client.CreateAsync(originKey, targetKey, rate);

        // Act
        var deleted = await _fixture.Client.DeleteAsync(created.Key);

        // Assert
        Assert.NotNull(deleted);
        Assert.Equal(created.Key, deleted.Key);
        Assert.Equal(originKey, deleted.Entity.OriginKey);
        Assert.Equal(targetKey, deleted.Entity.TargetKey);
        Assert.Equal(rate, deleted.Entity.Rate);
        Assert.NotNull(deleted.Revision);
        Assert.Equal(RevisionAction.Deleted, deleted.Revision.Action);
        Assert.Equal(RevisionStatus.Active, deleted.Revision.Status);
    }

    [Fact]
    public async Task DeleteCurrencyRate_UsingOriginAndTargetKeys_DeletesRate()
    {
        // Arrange
        var originKey = Guid.NewGuid();
        var targetKey = Guid.NewGuid();
        var rate = 1.5m;

        var created = await _fixture.Client.CreateAsync(originKey, targetKey, rate);

        // Act
        var deleted = await _fixture.Client.DeleteAsync(originKey, targetKey);

        // Assert
        Assert.NotNull(deleted);
        Assert.Equal(created.Key, deleted.Key);
        Assert.Equal(originKey, deleted.Entity.OriginKey);
        Assert.Equal(targetKey, deleted.Entity.TargetKey);
        Assert.Equal(rate, deleted.Entity.Rate);
        Assert.NotNull(deleted.Revision);
        Assert.Equal(RevisionAction.Deleted, deleted.Revision.Action);
        Assert.Equal(RevisionStatus.Active, deleted.Revision.Status);
    }

    [Fact]
    public async Task DeleteCurrencyRate_DeletedRate_CanBeRestored()
    {
        // Arrange
        var originKey = Guid.NewGuid();
        var targetKey = Guid.NewGuid();
        var rate = 1.5m;

        var created = await _fixture.Client.CreateAsync(originKey, targetKey, rate);
        await _fixture.Client.DeleteAsync(created.Key);

        // Act
        var restored = await _fixture.Client.CreateAsync(originKey, targetKey, rate);

        // Assert
        Assert.NotNull(restored);
        Assert.Equal(created.Key, restored.Key);
        Assert.Equal(originKey, restored.Entity.OriginKey);
        Assert.Equal(targetKey, restored.Entity.TargetKey);
        Assert.Equal(rate, restored.Entity.Rate);
        Assert.NotNull(restored.Revision);
        Assert.Equal(RevisionAction.Restored, restored.Revision.Action);
        Assert.Equal(RevisionStatus.Active, restored.Revision.Status);
    }

    [Fact]
    public async Task DeleteCurrencyRate_DeletedRate_CannotBeDeletedAgain()
    {
        // Arrange
        var originKey = Guid.NewGuid();
        var targetKey = Guid.NewGuid();
        var rate = 1.5m;

        var created = await _fixture.Client.CreateAsync(originKey, targetKey, rate);
        await _fixture.Client.DeleteAsync(created.Key);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            () => _fixture.Client.DeleteAsync(created.Key));

        Assert.Equal(StatusCode.NotFound, exception.StatusCode);
    }

    [Fact]
    public async Task DeleteCurrencyRate_DeletedRate_NotReturnedInGetAll()
    {
        // Arrange
        var originKey = Guid.NewGuid();
        var targetKey = Guid.NewGuid();
        var rate = 1.5m;

        var created = await _fixture.Client.CreateAsync(originKey, targetKey, rate);
        await _fixture.Client.DeleteAsync(created.Key);

        // Act
        var allRates = await _fixture.Client.GetAllAsync();

        // Assert
        Assert.DoesNotContain(allRates, r => r.Key == created.Key);
    }

    [Fact]
    public async Task DeleteCurrencyRate_DeletedRate_StillVisibleInRevisions()
    {
        // Arrange
        var originKey = Guid.NewGuid();
        var targetKey = Guid.NewGuid();
        var rate = 1.5m;

        var created = await _fixture.Client.CreateAsync(originKey, targetKey, rate);
        await _fixture.Client.DeleteAsync(created.Key);

        // Act
        var revisions = await _fixture.Client.GetAllRevisionsAsync(created.Key);

        // Assert
        Assert.NotEmpty(revisions);
        Assert.Contains(revisions, r => r.Revision.Action == RevisionAction.Created);
        Assert.Contains(revisions, r => r.Revision.Action == RevisionAction.Deleted);
    }
}