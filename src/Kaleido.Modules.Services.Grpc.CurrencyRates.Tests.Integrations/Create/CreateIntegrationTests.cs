using Grpc.Core;
using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Client.Models;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Integrations.Fixtures;
using Xunit;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Integrations.Create;

[Collection(nameof(InfrastructureCollection))]
public class CreateIntegrationTests : IAsyncLifetime
{
    private readonly InfrastructureFixture _fixture;

    public CreateIntegrationTests(InfrastructureFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _fixture.ClearDatabase();

    [Fact]
    public async Task CreateCurrencyRate_ValidRequest_CreatesNewRate()
    {
        // Arrange
        var originKey = Guid.NewGuid();
        var targetKey = Guid.NewGuid();
        var rate = 1.5m;

        // Act
        var result = await _fixture.Client.CreateAsync(originKey, targetKey, rate);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Key);
        Assert.Equal(originKey, result.Entity.OriginKey);
        Assert.Equal(targetKey, result.Entity.TargetKey);
        Assert.Equal(rate, result.Entity.Rate);
        Assert.NotNull(result.Revision);
        Assert.Equal(RevisionAction.Created, result.Revision.Action);
        Assert.Equal(RevisionStatus.Active, result.Revision.Status);
    }

    [Fact]
    public async Task CreateCurrencyRate_DuplicateConversion_ThrowsException()
    {
        // Arrange
        var originKey = Guid.NewGuid();
        var targetKey = Guid.NewGuid();
        var rate = 1.5m;

        // Act
        await _fixture.Client.CreateAsync(originKey, targetKey, rate);

        // Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            () => _fixture.Client.CreateAsync(originKey, targetKey, rate));

        Assert.Equal(StatusCode.AlreadyExists, exception.StatusCode);
    }

    [Fact]
    public async Task CreateCurrencyRate_DeletedConversion_RestoresEntity()
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
    public async Task CreateCurrencyRate_MultipleConversions_CreatesDistinctEntities()
    {
        // Arrange
        var originKey1 = Guid.NewGuid();
        var targetKey1 = Guid.NewGuid();
        var originKey2 = Guid.NewGuid();
        var targetKey2 = Guid.NewGuid();
        var rate = 1.5m;

        // Act
        var result1 = await _fixture.Client.CreateAsync(originKey1, targetKey1, rate);
        var result2 = await _fixture.Client.CreateAsync(originKey2, targetKey2, rate);

        // Assert
        Assert.NotEqual(result1.Key, result2.Key);
        Assert.Equal(originKey1, result1.Entity.OriginKey);
        Assert.Equal(targetKey1, result1.Entity.TargetKey);
        Assert.Equal(originKey2, result2.Entity.OriginKey);
        Assert.Equal(targetKey2, result2.Entity.TargetKey);
    }

    [Fact]
    public async Task CreateCurrencyRate_ReverseConversion_CreatesNewEntity()
    {
        // Arrange
        var originKey = Guid.NewGuid();
        var targetKey = Guid.NewGuid();
        var rate = 1.5m;

        // Act
        var forward = await _fixture.Client.CreateAsync(originKey, targetKey, rate);
        var reverse = await _fixture.Client.CreateAsync(targetKey, originKey, 1 / rate);

        // Assert
        Assert.NotEqual(forward.Key, reverse.Key);
        Assert.Equal(originKey, forward.Entity.OriginKey);
        Assert.Equal(targetKey, forward.Entity.TargetKey);
        Assert.Equal(targetKey, reverse.Entity.OriginKey);
        Assert.Equal(originKey, reverse.Entity.TargetKey);
    }

    [Fact]
    public async Task CreateCurrencyRate_InvalidRate_ThrowsException()
    {
        // Arrange
        var originKey = Guid.NewGuid();
        var targetKey = Guid.NewGuid();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            () => _fixture.Client.CreateAsync(originKey, targetKey, -1));

        Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
    }

    [Fact]
    public async Task CreateCurrencyRate_ZeroRate_ShouldBeValid()
    {
        // Arrange
        var originKey = Guid.NewGuid();
        var targetKey = Guid.NewGuid();
        var rate = 0m;

        // Act
        var result = await _fixture.Client.CreateAsync(originKey, targetKey, rate);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Key);
        Assert.Equal(originKey, result.Entity.OriginKey);
        Assert.Equal(targetKey, result.Entity.TargetKey);
        Assert.Equal(rate, result.Entity.Rate);
        Assert.NotNull(result.Revision);
        Assert.Equal(RevisionAction.Created, result.Revision.Action);
        Assert.Equal(RevisionStatus.Active, result.Revision.Status);
    }

    [Fact]
    public async Task CreateCurrencyRate_SameOriginAndTarget_ThrowsException()
    {
        // Arrange
        var key = Guid.NewGuid();
        var rate = 1.5m;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            () => _fixture.Client.CreateAsync(key, key, rate));

        Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
    }

    [Fact]
    public async Task CreateCurrencyRate_UsingEntityDto_CreatesNewRate()
    {
        // Arrange
        var originKey = Guid.NewGuid();
        var targetKey = Guid.NewGuid();
        var rate = 1.5m;

        var createDto = new CurrencyRateEntityDto
        {
            OriginKey = originKey,
            TargetKey = targetKey,
            Rate = rate
        };

        // Act
        var result = await _fixture.Client.CreateAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Key);
        Assert.Equal(originKey, result.Entity.OriginKey);
        Assert.Equal(targetKey, result.Entity.TargetKey);
        Assert.Equal(rate, result.Entity.Rate);
        Assert.Equal(RevisionAction.Created, result.Revision.Action);
        Assert.Equal(RevisionStatus.Active, result.Revision.Status);
    }

    [Fact]
    public async Task CreateCurrencyRate_UsingEntityDtoWithInvalidRate_ThrowsException()
    {
        // Arrange
        var createDto = new CurrencyRateEntityDto
        {
            OriginKey = Guid.NewGuid(),
            TargetKey = Guid.NewGuid(),
            Rate = -1
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            () => _fixture.Client.CreateAsync(createDto));
        Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
    }

    [Fact]
    public async Task CreateCurrencyRate_UsingEntityDtoWithSameKeys_ThrowsException()
    {
        // Arrange
        var key = Guid.NewGuid();
        var createDto = new CurrencyRateEntityDto
        {
            OriginKey = key,
            TargetKey = key,
            Rate = 1.5m
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            () => _fixture.Client.CreateAsync(createDto));
        Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
    }
}
