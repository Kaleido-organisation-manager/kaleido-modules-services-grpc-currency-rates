using AutoMapper;
using Grpc.Core;
using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Grpc.CurrencyRates;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Mappers;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Models;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Validators;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Unit.Builders;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Update;
using Moq;
using Moq.AutoMock;
using Xunit;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Unit.Update;

public class UpdateHandlerTests
{
    private readonly AutoMocker _mocker;
    private readonly UpdateHandler _sut;
    private readonly CurrencyRateActionRequest _validRequest;
    private readonly Guid _validKey;

    public UpdateHandlerTests()
    {
        _mocker = new AutoMocker();
        _validKey = Guid.NewGuid();
        _validRequest = new CurrencyRateActionRequestBuilder()
            .WithKey(_validKey)
            .Build();

        // Happy path setup
        _mocker.Use(new KeyValidator());
        _mocker.Use(new CurrencyRateValidator());

        var mapper = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<CurrencyRateMappingProfile>();
        });
        _mocker.Use(mapper.CreateMapper());

        var successResponse = ManagerResponse.Success(
            new EntityLifeCycleResult<CurrencyRateEntity, BaseRevisionEntity>
            {
                Entity = new CurrencyRateBuilder().BuildEntity(),
                Revision = new BaseRevisionEntity { Action = RevisionAction.Updated }
            });

        _mocker.GetMock<IUpdateManager>()
            .Setup(m => m.UpdateAsync(
                _validKey,
                It.IsAny<CurrencyRateEntity>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        _sut = _mocker.CreateInstance<UpdateHandler>();
    }

    [Fact]
    public async Task HandleAsync_ValidRequest_ReturnsCurrencyRateResponse()
    {
        // Act
        var result = await _sut.HandleAsync(_validRequest);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<CurrencyRateResponse>(result);
    }

    [Fact]
    public async Task HandleAsync_InvalidKeyFormat_ThrowsRpcException()
    {
        // Arrange
        var invalidRequest = new CurrencyRateActionRequestBuilder()
            .WithKey("invalid-guid")
            .Build();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            () => _sut.HandleAsync(invalidRequest));
        Assert.Equal(StatusCode.InvalidArgument, exception.Status.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_EmptyKey_ThrowsRpcException()
    {
        // Arrange
        var emptyRequest = new CurrencyRateActionRequestBuilder()
            .WithKey(string.Empty)
            .Build();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            () => _sut.HandleAsync(emptyRequest));
        Assert.Equal(StatusCode.InvalidArgument, exception.Status.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_InvalidCurrencyRate_ThrowsRpcException()
    {
        // Arrange
        var invalidCurrencyRate = new CurrencyRate(); // Empty currency rate
        var request = new CurrencyRateActionRequestBuilder()
            .WithKey(_validKey)
            .WithCurrencyRate(invalidCurrencyRate)
            .Build();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            () => _sut.HandleAsync(request));
        Assert.Equal(StatusCode.InvalidArgument, exception.Status.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_ManagerReturnsNotFound_ThrowsRpcException()
    {
        // Arrange
        _mocker.GetMock<IUpdateManager>()
            .Setup(m => m.UpdateAsync(
                _validKey,
                It.IsAny<CurrencyRateEntity>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ManagerResponse.NotFound());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            () => _sut.HandleAsync(_validRequest));
        Assert.Equal(StatusCode.NotFound, exception.Status.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_ManagerReturnsNotModified_ThrowsRpcException()
    {
        // Arrange
        _mocker.GetMock<IUpdateManager>()
            .Setup(m => m.UpdateAsync(
                _validKey,
                It.IsAny<CurrencyRateEntity>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ManagerResponse.NotModified());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            () => _sut.HandleAsync(_validRequest));
        Assert.Equal(StatusCode.AlreadyExists, exception.Status.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_ManagerThrowsException_ThrowsRpcException()
    {
        // Arrange
        _mocker.GetMock<IUpdateManager>()
            .Setup(m => m.UpdateAsync(
                _validKey,
                It.IsAny<CurrencyRateEntity>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            () => _sut.HandleAsync(_validRequest));
        Assert.Equal(StatusCode.Internal, exception.Status.StatusCode);
    }
}