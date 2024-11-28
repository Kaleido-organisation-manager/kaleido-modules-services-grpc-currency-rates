using Grpc.Core;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Services;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Create;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Delete;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Get;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Unit.Builders;
using Moq;
using Moq.AutoMock;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Unit.Common.Services;

public class CurrencyRateServiceTests
{
    private readonly AutoMocker _mocker;
    private readonly CurrencyRateService _sut;

    public CurrencyRateServiceTests()
    {
        _mocker = new AutoMocker();
        _sut = _mocker.CreateInstance<CurrencyRateService>();
    }

    [Fact]
    public async Task CreateCurrencyRate_CallsHandleAsyncOnCreateHandler()
    {
        // Arrange
        var request = new CurrencyRateBuilder().Build();
        var context = new Mock<ServerCallContext>().Object;

        // Act
        await _sut.CreateCurrencyRate(request, context);

        // Assert
        _mocker.GetMock<ICreateHandler>()
            .Verify(x => x.HandleAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteCurrencyRate_CallsHandleAsyncOnDeleteHandler()
    {
        // Arrange
        var request = new CurrencyRateRequestBuilder().Build();
        var context = new Mock<ServerCallContext>().Object;

        // Act
        await _sut.DeleteCurrencyRate(request, context);

        // Assert
        _mocker.GetMock<IDeleteHandler>()
            .Verify(x => x.HandleAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCurrencyRate_CallsHandleAsyncOnGetHandler()
    {
        // Arrange
        var request = new CurrencyRateRequestBuilder().Build();
        var context = new Mock<ServerCallContext>().Object;

        // Act
        await _sut.GetCurrencyRate(request, context);

        // Assert
        _mocker.GetMock<IGetHandler>()
            .Verify(x => x.HandleAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }
}
