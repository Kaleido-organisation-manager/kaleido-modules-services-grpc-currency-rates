using AutoMapper;
using Grpc.Core;
using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Grpc.CurrencyRates;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Mappers;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Models;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Validators;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Delete;
using Moq;
using Moq.AutoMock;
using Xunit;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Unit.Builders;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Unit.Delete;

public class DeleteHandlerTests
{
    private readonly AutoMocker _mocker;
    private readonly DeleteHandler _sut;
    private readonly CurrencyRateRequest _validRequest;
    private readonly Guid _validKey;

    public DeleteHandlerTests()
    {
        _mocker = new AutoMocker();
        _validKey = Guid.NewGuid();
        _validRequest = new CurrencyRateRequestBuilder()
            .WithKey(_validKey)
            .Build();

        // Happy path setup
        _mocker.Use(new KeyValidator());

        var mapper = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<CurrencyRateMappingProfile>();
        });
        _mocker.Use(mapper.CreateMapper());

        var successResponse = ManagerResponse.Success(
            new EntityLifeCycleResult<CurrencyRateEntity, BaseRevisionEntity>
            {
                Entity = new CurrencyRateEntity { OriginKey = Guid.NewGuid(), TargetKey = Guid.NewGuid(), Rate = 0.85m },
                Revision = new BaseRevisionEntity { Action = RevisionAction.Deleted }
            });

        _mocker.GetMock<IDeleteManager>()
            .Setup(m => m.DeleteAsync(_validKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        _sut = _mocker.CreateInstance<DeleteHandler>();
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
        var invalidRequest = new CurrencyRateRequestBuilder()
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
        var emptyRequest = new CurrencyRateRequestBuilder()
            .WithKey(string.Empty)
            .Build();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            () => _sut.HandleAsync(emptyRequest));
        Assert.Equal(StatusCode.InvalidArgument, exception.Status.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_ManagerReturnsNotFound_ThrowsRpcException()
    {
        // Arrange
        _mocker.GetMock<IDeleteManager>()
            .Setup(m => m.DeleteAsync(_validKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ManagerResponse.NotFound());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            () => _sut.HandleAsync(_validRequest));
        Assert.Equal(StatusCode.NotFound, exception.Status.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_ManagerThrowsException_ThrowsRpcException()
    {
        // Arrange
        _mocker.GetMock<IDeleteManager>()
            .Setup(m => m.DeleteAsync(_validKey, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            () => _sut.HandleAsync(_validRequest));
        Assert.Equal(StatusCode.Internal, exception.Status.StatusCode);
    }
}