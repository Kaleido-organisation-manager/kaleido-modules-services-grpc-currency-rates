using Grpc.Core;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Services;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Create;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Delete;
using Kaleido.Modules.Services.Grpc.CurrencyRates.GetAllConversions;
using Kaleido.Modules.Services.Grpc.CurrencyRates.GetAllRevisions;
using Kaleido.Modules.Services.Grpc.CurrencyRates.GetConversion;
using Kaleido.Modules.Services.Grpc.CurrencyRates.GetRevision;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Update;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Unit.Builders;
using Moq;
using Moq.AutoMock;
using Xunit;

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
    public async Task UpdateCurrencyRate_CallsHandleAsyncOnUpdateHandler()
    {
        // Arrange
        var request = new CurrencyRateActionRequestBuilder().Build();
        var context = new Mock<ServerCallContext>().Object;

        // Act
        await _sut.UpdateCurrencyRate(request, context);

        // Assert
        _mocker.GetMock<IUpdateHandler>()
            .Verify(x => x.HandleAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllCurrencyConversions_CallsHandleAsyncOnGetAllConversionsHandler()
    {
        // Arrange
        var request = new CurrencyRateRequestBuilder().Build();
        var context = new Mock<ServerCallContext>().Object;

        // Act
        await _sut.GetAllCurrencyConversions(request, context);

        // Assert
        _mocker.GetMock<IGetAllConversionsHandler>()
            .Verify(x => x.HandleAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllCurrencyRateRevisions_CallsHandleAsyncOnGetAllRevisionsHandler()
    {
        // Arrange
        var request = new CurrencyRateRequestBuilder().Build();
        var context = new Mock<ServerCallContext>().Object;

        // Act
        await _sut.GetAllCurrencyRateRevisions(request, context);

        // Assert
        _mocker.GetMock<IGetAllRevisionsHandler>()
            .Verify(x => x.HandleAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCurrencyConversion_CallsHandleAsyncOnGetConversionHandler()
    {
        // Arrange
        var request = new CurrencyConversionRequestBuilder().Build();
        var context = new Mock<ServerCallContext>().Object;

        // Act
        await _sut.GetCurrencyConversion(request, context);

        // Assert
        _mocker.GetMock<IGetConversionHandler>()
            .Verify(x => x.HandleAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCurrencyRateRevision_CallsHandleAsyncOnGetRevisionHandler()
    {
        // Arrange
        var request = new CurrencyRateRevisionRequestBuilder().Build();
        var context = new Mock<ServerCallContext>().Object;

        // Act
        await _sut.GetCurrencyRateRevision(request, context);

        // Assert
        _mocker.GetMock<IGetRevisionHandler>()
            .Verify(x => x.HandleAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }
}
