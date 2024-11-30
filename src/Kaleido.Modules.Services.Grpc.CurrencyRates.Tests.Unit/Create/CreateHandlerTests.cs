using AutoMapper;
using Grpc.Core;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Grpc.CurrencyRates;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Mappers;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Models;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Validators;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Create;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Unit.Builders;
using Moq;
using Moq.AutoMock;
using Xunit;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Unit.Create;

public class CreateHandlerTests
{
    private readonly AutoMocker _mocker;
    private readonly CreateHandler _sut;

    public CreateHandlerTests()
    {
        _mocker = new AutoMocker();

        // Happy path setup
        _mocker.Use(new CurrencyRateValidator());

        var mapper = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<CurrencyRateMappingProfile>();
        });
        _mocker.Use(mapper.CreateMapper());

        _mocker.GetMock<ICreateManager>()
            .Setup(m => m.CreateAsync(It.IsAny<CurrencyRateEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CurrencyRateEntity entity, CancellationToken cancellationToken) => ManagerResponse.Success(
                new EntityLifeCycleResult<CurrencyRateEntity, BaseRevisionEntity>
                {
                    Entity = entity,
                    Revision = new BaseRevisionEntity()
                }
            ));

        _sut = _mocker.CreateInstance<CreateHandler>();
    }

    [Fact]
    public async Task HandleAsync_ValidRequest_ReturnsCurrencyRateResponse()
    {
        // Arrange
        var request = new CurrencyRateBuilder().Build();

        // Act
        var result = await _sut.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<CurrencyRateResponse>(result);
        Assert.Equal(request.OriginKey, result.CurrencyRate?.OriginKey);
        Assert.Equal(request.TargetKey, result.CurrencyRate?.TargetKey);
    }

    [Fact]
    public async Task HandleAsync_ValidRequest_CallsValidatorAndManager()
    {
        // Arrange
        var request = new CurrencyRateBuilder().Build();

        // Act
        await _sut.HandleAsync(request, CancellationToken.None);

        // Assert
        _mocker.GetMock<ICreateManager>()
            .Verify(m => m.CreateAsync(It.IsAny<CurrencyRateEntity>(), CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ValidationFails_ThrowsRpcException()
    {
        // Arrange
        var invalidRequest = new CurrencyRate(); // Empty request

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            () => _sut.HandleAsync(invalidRequest, CancellationToken.None));
        Assert.Equal(StatusCode.InvalidArgument, exception.Status.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_ManagerThrowsException_ThrowsRpcException()
    {
        // Arrange
        var request = new CurrencyRateBuilder().Build();
        _mocker.GetMock<ICreateManager>()
            .Setup(m => m.CreateAsync(It.IsAny<CurrencyRateEntity>(), CancellationToken.None))
            .ThrowsAsync(new Exception("Test exception"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            () => _sut.HandleAsync(request, CancellationToken.None));
        Assert.Equal(StatusCode.Internal, exception.Status.StatusCode);
    }
}