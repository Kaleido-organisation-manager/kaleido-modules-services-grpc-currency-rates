using AutoMapper;
using Grpc.Core;
using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Grpc.CurrencyRates;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Mappers;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Models;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Validators;
using Kaleido.Modules.Services.Grpc.CurrencyRates.GetRevision;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Unit.Builders;
using Moq;
using Moq.AutoMock;
using Xunit;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Unit.GetRevision;

public class GetRevisionHandlerTests
{
    private readonly AutoMocker _mocker;
    private readonly GetRevisionHandler _sut;
    private readonly CurrencyRateRevisionRequest _validRequest;
    private readonly Guid _validKey;
    private readonly DateTime _validCreatedAt;

    public GetRevisionHandlerTests()
    {
        _mocker = new AutoMocker();
        _validKey = Guid.NewGuid();
        _validCreatedAt = DateTime.UtcNow;
        _validRequest = new CurrencyRateRevisionRequestBuilder()
            .WithKey(_validKey)
            .WithCreatedAt(_validCreatedAt)
            .Build();

        // Happy path setup
        _mocker.Use(new KeyValidator());

        var mapper = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<CurrencyRateMappingProfile>();
        });
        _mocker.Use(mapper.CreateMapper());

        var entityResult = new EntityLifeCycleResult<CurrencyRateEntity, BaseRevisionEntity>
        {
            Entity = new CurrencyRateBuilder().BuildEntity(),
            Revision = new BaseRevisionEntity { Action = RevisionAction.Created }
        };

        _mocker.GetMock<IGetRevisionManager>()
            .Setup(m => m.GetRevisionAsync(_validKey, _validCreatedAt, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ManagerResponse.Success(entityResult));

        _sut = _mocker.CreateInstance<GetRevisionHandler>();
    }

    [Fact]
    public async Task HandleAsync_ValidRequest_ReturnsCurrencyRateResponse()
    {
        // Act
        var result = await _sut.HandleAsync(_validRequest, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<CurrencyRateResponse>(result);
    }

    [Fact]
    public async Task HandleAsync_InvalidKeyFormat_ThrowsRpcException()
    {
        // Arrange
        var invalidRequest = new CurrencyRateRevisionRequestBuilder()
            .WithKey("invalid-guid")
            .Build();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            () => _sut.HandleAsync(invalidRequest, CancellationToken.None));
        Assert.Equal(StatusCode.InvalidArgument, exception.Status.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_EmptyKey_ThrowsRpcException()
    {
        // Arrange
        var emptyRequest = new CurrencyRateRevisionRequestBuilder()
            .WithKey(string.Empty)
            .Build();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            () => _sut.HandleAsync(emptyRequest, CancellationToken.None));
        Assert.Equal(StatusCode.InvalidArgument, exception.Status.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_RevisionNotFound_ThrowsRpcException()
    {
        // Arrange
        _mocker.GetMock<IGetRevisionManager>()
            .Setup(m => m.GetRevisionAsync(_validKey, _validCreatedAt, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ManagerResponse.NotFound());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            () => _sut.HandleAsync(_validRequest, CancellationToken.None));
        Assert.Equal(StatusCode.NotFound, exception.Status.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_ManagerThrowsException_ThrowsRpcException()
    {
        // Arrange
        _mocker.GetMock<IGetRevisionManager>()
            .Setup(m => m.GetRevisionAsync(_validKey, _validCreatedAt, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            () => _sut.HandleAsync(_validRequest, CancellationToken.None));
        Assert.Equal(StatusCode.Internal, exception.Status.StatusCode);
    }
}