using AutoMapper;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Kaleido.Grpc.CurrencyRates;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Client.Models;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Client.Mapping;

public class CurrencyRateDtoMappingProfile : Profile
{
    public CurrencyRateDtoMappingProfile()
    {
        CreateMap<CurrencyRateResponse, CurrencyRateDto>()
            .ForMember(dest => dest.Key, opt => opt.MapFrom(src => src.Key))
            .ForMember(dest => dest.Entity, opt => opt.MapFrom(src => src.CurrencyRate))
            .ForMember(dest => dest.Revision, opt => opt.MapFrom(src => src.Revision));

        CreateMap<CurrencyRate, CurrencyRateEntityDto>()
            .ForMember(dest => dest.Rate, opt => opt.MapFrom(src =>
                Math.Round((decimal)src.Rate, 2, MidpointRounding.AwayFromZero)));

        CreateMap<BaseRevision, CurrencyRateRevisionDto>();

        CreateMap<CurrencyRateListResponse, IEnumerable<CurrencyRateDto>>()
           .ConvertUsing((src, dest, context) =>
               src.CurrencyRates.Select(rate => context.Mapper.Map<CurrencyRateDto>(rate)));


        // DateTime <-> Timestamp conversions
        CreateMap<Timestamp, DateTime>().ConvertUsing(src => src.ToDateTime());
        CreateMap<DateTime, Timestamp>().ConvertUsing(src => Timestamp.FromDateTime(src.ToUniversalTime()));

        // Guid <-> string conversions
        CreateMap<Guid, string>().ConvertUsing(src => src.ToString());
        CreateMap<string, Guid>().ConvertUsing(src => Guid.Parse(src));
    }
}
