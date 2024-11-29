using AutoMapper;
using Grpc.Core;
using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Grpc.CurrencyRates;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Mappers;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Models;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Validators;
using Kaleido.Modules.Services.Grpc.CurrencyRates.GetAllRevisions;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Unit.Builders;
using Moq;
using Moq.AutoMock;
using Xunit;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Unit.GetAllRevisions;

public class GetAllRevisionsHandlerTests
{
    private readonly AutoMocker _mocker;
    private readonly GetAllRevisionsHandler _sut;
    private readonly CurrencyRateRequest _validRequest;
    private readonly Guid _validKey;

    public GetAllRevisionsHandlerTests()
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

        var entityResult = new EntityLifeCycleResult<CurrencyRateEntity, BaseRevisionEntity>
        {
            Entity = new CurrencyRateBuilder().BuildEntity(),
            Revision = new BaseRevisionEntity { Action = RevisionAction.Created }
        };

        _mocker.GetMock<IGetAllRevisionsManager>()
            .Setup(m => m.GetAllRevisionsAsync(_validKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { ManagerResponse.Success(entityResult) });

        _sut = _mocker.CreateInstance<GetAllRevisionsHandler>();
    }

    [Fact]
    public async Task HandleAsync_ValidRequest_ReturnsCurrencyRateListResponse()
    {
        // Act
        var result = await _sut.HandleAsync(_validRequest, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<CurrencyRateListResponse>(result);
        Assert.NotEmpty(result.CurrencyRates);
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
            () => _sut.HandleAsync(invalidRequest, CancellationToken.None));
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
            () => _sut.HandleAsync(emptyRequest, CancellationToken.None));
        Assert.Equal(StatusCode.InvalidArgument, exception.Status.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_NoRevisionsFound_ReturnsEmptyList()
    {
        // Arrange
        _mocker.GetMock<IGetAllRevisionsManager>()
            .Setup(m => m.GetAllRevisionsAsync(_validKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<ManagerResponse>());

        // Act
        var result = await _sut.HandleAsync(_validRequest, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.CurrencyRates);
    }

    [Fact]
    public async Task HandleAsync_ManagerThrowsException_ThrowsRpcException()
    {
        // Arrange
        _mocker.GetMock<IGetAllRevisionsManager>()
            .Setup(m => m.GetAllRevisionsAsync(_validKey, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            () => _sut.HandleAsync(_validRequest, CancellationToken.None));
        Assert.Equal(StatusCode.Internal, exception.Status.StatusCode);
    }
}