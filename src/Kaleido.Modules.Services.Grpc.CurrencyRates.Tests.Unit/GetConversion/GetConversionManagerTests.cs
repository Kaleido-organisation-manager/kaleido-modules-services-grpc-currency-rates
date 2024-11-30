using System.Linq.Expressions;
using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Exceptions;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Constants;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Models;
using Kaleido.Modules.Services.Grpc.CurrencyRates.GetConversion;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Unit.Builders;
using Moq;
using Moq.AutoMock;
using Xunit;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Unit.GetConversion;

public class GetConversionManagerTests
{
    private readonly AutoMocker _mocker;
    private readonly GetConversionManager _sut;
    private readonly Guid _validOriginKey;
    private readonly Guid _validTargetKey;
    private readonly EntityLifeCycleResult<CurrencyRateEntity, BaseRevisionEntity> _entityResult;

    public GetConversionManagerTests()
    {
        _mocker = new AutoMocker();
        _validOriginKey = Guid.NewGuid();
        _validTargetKey = Guid.NewGuid();

        var entity = new CurrencyRateBuilder().BuildEntity();
        _entityResult = new EntityLifeCycleResult<CurrencyRateEntity, BaseRevisionEntity>
        {
            Entity = entity,
            Revision = new BaseRevisionEntity
            {
                Action = RevisionAction.Created,
                Status = RevisionStatus.Active
            }
        };

        _mocker.GetMock<IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity>>()
            .Setup(h => h.FindAsync(
                It.IsAny<Expression<Func<CurrencyRateEntity, bool>>>(),
                It.IsAny<Expression<Func<BaseRevisionEntity, bool>>>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { _entityResult });

        _sut = _mocker.CreateInstance<GetConversionManager>();
    }

    [Fact]
    public async Task GetConversionAsync_ValidKeys_CallsEntityLifecycleHandler()
    {
        // Act
        await _sut.GetConversionAsync(_validOriginKey, _validTargetKey, CancellationToken.None);

        // Assert
        _mocker.GetMock<IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity>>()
            .Verify(h => h.FindAsync(
                It.IsAny<Expression<Func<CurrencyRateEntity, bool>>>(),
                It.IsAny<Expression<Func<BaseRevisionEntity, bool>>>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetConversionAsync_ValidKeys_ReturnsSuccessResponse()
    {
        // Act
        var result = await _sut.GetConversionAsync(_validOriginKey, _validTargetKey, CancellationToken.None);

        // Assert
        Assert.Equal(ManagerResponseState.Success, result.State);
        Assert.Equal(_entityResult, result.CurrencyRate);
    }

    [Fact]
    public async Task GetConversionAsync_NoConversionFound_ReturnsNotFoundResponse()
    {
        // Arrange
        _mocker.GetMock<IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity>>()
            .Setup(h => h.FindAsync(
                It.IsAny<Expression<Func<CurrencyRateEntity, bool>>>(),
                It.IsAny<Expression<Func<BaseRevisionEntity, bool>>>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<EntityLifeCycleResult<CurrencyRateEntity, BaseRevisionEntity>>());

        // Act
        var result = await _sut.GetConversionAsync(_validOriginKey, _validTargetKey, CancellationToken.None);

        // Assert
        Assert.Equal(ManagerResponseState.NotFound, result.State);
    }

    [Fact]
    public async Task GetConversionAsync_RevisionNotFound_ReturnsNotFoundResponse()
    {
        // Arrange
        _mocker.GetMock<IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity>>()
            .Setup(h => h.FindAsync(
                It.IsAny<Expression<Func<CurrencyRateEntity, bool>>>(),
                It.IsAny<Expression<Func<BaseRevisionEntity, bool>>>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RevisionNotFoundException("Test exception"));

        // Act
        var result = await _sut.GetConversionAsync(_validOriginKey, _validTargetKey, CancellationToken.None);

        // Assert
        Assert.Equal(ManagerResponseState.NotFound, result.State);
    }

    [Fact]
    public async Task GetConversionAsync_FiltersCorrectly()
    {
        // Act
        await _sut.GetConversionAsync(_validOriginKey, _validTargetKey, CancellationToken.None);

        // Assert
        _mocker.GetMock<IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity>>()
            .Verify(h => h.FindAsync(
                It.Is<Expression<Func<CurrencyRateEntity, bool>>>(f =>
                    f.Body.ToString()!.Contains(nameof(CurrencyRateEntity.OriginKey)) &&
                    f.Body.ToString()!.Contains(nameof(CurrencyRateEntity.TargetKey))),
                It.Is<Expression<Func<BaseRevisionEntity, bool>>>(f =>
                    f.Body.ToString()!.Contains(nameof(BaseRevisionEntity.Action)) &&
                    f.Body.ToString()!.Contains(nameof(BaseRevisionEntity.Status))),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()),
                Times.Once);
    }
}