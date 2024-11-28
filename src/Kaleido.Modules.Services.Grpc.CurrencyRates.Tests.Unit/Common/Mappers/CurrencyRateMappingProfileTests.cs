using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Grpc.CurrencyRates;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Mappers;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Models;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Unit.Builders;
using Xunit;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Unit.Common.Mappers;

public class CurrencyRateMappingProfileTests
{
    private readonly IMapper _mapper;

    public CurrencyRateMappingProfileTests()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<CurrencyRateMappingProfile>();
        });

        _mapper = configuration.CreateMapper();
    }

    [Fact]
    public void Configuration_IsValid()
    {
        // Assert
        _mapper.ConfigurationProvider.AssertConfigurationIsValid();
    }

    [Fact]
    public void Map_CurrencyRateToCurrencyRateEntity_MapsCorrectly()
    {
        // Arrange
        var source = new CurrencyRateBuilder().Build();

        // Act
        var result = _mapper.Map<CurrencyRateEntity>(source);

        // Assert
        Assert.Equal(Guid.Parse(source.OriginKey), result.OriginKey);
        Assert.Equal(Guid.Parse(source.TargetKey), result.TargetKey);
        Assert.Equal((decimal)source.Rate, result.Rate);
    }

    [Fact]
    public void Map_CurrencyRateEntityToCurrencyRate_MapsCorrectly()
    {
        // Arrange
        var source = new CurrencyRateBuilder().BuildEntity();

        // Act
        var result = _mapper.Map<CurrencyRate>(source);

        // Assert
        Assert.Equal(source.OriginKey.ToString(), result.OriginKey);
        Assert.Equal(source.TargetKey.ToString(), result.TargetKey);
        Assert.Equal((double)source.Rate, result.Rate);
    }

    [Fact]
    public void Map_EntityLifeCycleResultToCurrencyRateResponse_MapsCorrectly()
    {
        // Arrange
        var entity = new CurrencyRateBuilder().BuildEntity();
        var source = new EntityLifeCycleResult<CurrencyRateEntity, BaseRevisionEntity>
        {
            Entity = entity,
            Revision = new BaseRevisionEntity
            {
                Id = Guid.NewGuid(),
                Key = Guid.NewGuid(),
                Action = RevisionAction.Created
            }
        };

        // Act
        var result = _mapper.Map<CurrencyRateResponse>(source);

        // Assert
        Assert.NotNull(result.CurrencyRate);
        Assert.NotNull(result.Revision);
        Assert.Equal(source.Entity.OriginKey.ToString(), result.CurrencyRate.OriginKey);
        Assert.Equal(source.Entity.TargetKey.ToString(), result.CurrencyRate.TargetKey);
        Assert.Equal((double)source.Entity.Rate, result.CurrencyRate.Rate);
        Assert.Equal(source.Revision.Key.ToString(), result.Revision.Key);
        Assert.Equal(source.Revision.Action.ToString(), result.Revision.Action);
    }

    [Fact]
    public void Map_DateTimeToTimestamp_MapsCorrectly()
    {
        // Arrange
        var dateTime = DateTime.UtcNow;

        // Act
        var timestamp = _mapper.Map<Timestamp>(dateTime);
        var roundTrip = _mapper.Map<DateTime>(timestamp);

        // Assert
        Assert.Equal(dateTime.ToUniversalTime(), roundTrip.ToUniversalTime(), TimeSpan.FromSeconds(1));
    }
}