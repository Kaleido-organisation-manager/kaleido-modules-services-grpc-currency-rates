using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Grpc.CurrencyRates;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Models;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Mappers;


public class CurrencyRateMappingProfile : Profile
{
    public CurrencyRateMappingProfile()
    {
        // Basic entity mappings
        CreateMap<CurrencyRate, CurrencyRateEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Rate, opt => opt.MapFrom(src =>
                Math.Round((decimal)src.Rate, 2, MidpointRounding.AwayFromZero)));

        CreateMap<CurrencyRateEntity, CurrencyRate>()
            .ForMember(dest => dest.Rate, opt => opt.MapFrom(src =>
                Math.Round(src.Rate, 2, MidpointRounding.AwayFromZero)));

        // Response mappings
        CreateMap<EntityLifeCycleResult<CurrencyRateEntity, BaseRevisionEntity>, CurrencyRateResponse>()
            .ForMember(dest => dest.CurrencyRate, opt => opt.MapFrom(src => src.Entity))
            .ForMember(dest => dest.Revision, opt => opt.MapFrom(src => src.Revision));

        // Revision mappings
        CreateMap<BaseRevisionEntity, BaseRevision>();

        // DateTime <-> Timestamp conversions
        CreateMap<Timestamp, DateTime>().ConvertUsing(src => src.ToDateTime());
        CreateMap<DateTime, Timestamp>().ConvertUsing(src => Timestamp.FromDateTime(src.ToUniversalTime()));
    }
}