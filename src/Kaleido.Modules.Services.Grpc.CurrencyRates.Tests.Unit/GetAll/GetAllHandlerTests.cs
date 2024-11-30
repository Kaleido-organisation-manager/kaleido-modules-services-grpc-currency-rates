using AutoMapper;
using Grpc.Core;
using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Grpc.CurrencyRates;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Mappers;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Models;
using Kaleido.Modules.Services.Grpc.CurrencyRates.GetAll;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Unit.Builders;
using Moq;
using Moq.AutoMock;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Unit.GetAll;

public class GetAllHandlerTests
{
    private readonly AutoMocker _mocker;
    private readonly GetAllHandler _sut;
    private readonly EmptyRequest _request;

    public GetAllHandlerTests()
    {
        _mocker = new AutoMocker();
        _request = new EmptyRequest();

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

        _mocker.GetMock<IGetAllManager>()
            .Setup(m => m.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { ManagerResponse.Success(entityResult) });

        _sut = _mocker.CreateInstance<GetAllHandler>();
    }

    [Fact]
    public async Task HandleAsync_ValidRequest_ReturnsCurrencyRateListResponse()
    {
        // Act
        var result = await _sut.HandleAsync(_request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<CurrencyRateListResponse>(result);
        Assert.NotEmpty(result.CurrencyRates);
    }

    [Fact]
    public async Task HandleAsync_NoResults_ReturnsEmptyList()
    {
        // Arrange
        _mocker.GetMock<IGetAllManager>()
            .Setup(m => m.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<ManagerResponse>());

        // Act
        var result = await _sut.HandleAsync(_request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.CurrencyRates);
    }

    [Fact]
    public async Task HandleAsync_ManagerThrowsException_ThrowsRpcException()
    {
        // Arrange
        _mocker.GetMock<IGetAllManager>()
            .Setup(m => m.GetAllAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            () => _sut.HandleAsync(_request, CancellationToken.None));
        Assert.Equal(StatusCode.Internal, exception.Status.StatusCode);
    }
}