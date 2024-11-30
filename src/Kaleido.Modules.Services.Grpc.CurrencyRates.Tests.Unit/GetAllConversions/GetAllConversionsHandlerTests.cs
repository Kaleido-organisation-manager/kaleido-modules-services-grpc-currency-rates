using AutoMapper;
using Grpc.Core;
using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Grpc.CurrencyRates;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Mappers;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Models;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Validators;
using Kaleido.Modules.Services.Grpc.CurrencyRates.GetAllConversions;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Unit.Builders;
using Moq;
using Moq.AutoMock;
using Xunit;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Unit.GetAllConversions;

public class GetAllConversionsHandlerTests
{
    private readonly AutoMocker _mocker;
    private readonly GetAllConversionsHandler _sut;
    private readonly CurrencyRateRequest _validRequest;
    private readonly Guid _validKey;

    public GetAllConversionsHandlerTests()
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

        _mocker.GetMock<IGetAllConversionsManager>()
            .Setup(m => m.GetAllConversionsAsync(_validKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { ManagerResponse.Success(entityResult) });

        _sut = _mocker.CreateInstance<GetAllConversionsHandler>();
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
    public async Task HandleAsync_NoConversionsFound_ReturnsEmptyList()
    {
        // Arrange
        _mocker.GetMock<IGetAllConversionsManager>()
            .Setup(m => m.GetAllConversionsAsync(_validKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<ManagerResponse>());

        // Act
        var result = await _sut.HandleAsync(_validRequest, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.CurrencyRates);
    }
}