using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Kaleido.Grpc.CurrencyRates;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Client.Models;
using static Kaleido.Grpc.CurrencyRates.GrpcCurrencyRateService;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Client.Client;

public class CurrencyRateClient : ICurrencyRateClient
{
    private readonly GrpcCurrencyRateServiceClient _client;
    private readonly IMapper _mapper;

    public CurrencyRateClient(
        GrpcCurrencyRateServiceClient client,
        IMapper mapper)
    {
        _client = client;
        _mapper = mapper;
    }

    public async Task<CurrencyRateDto> CreateAsync(Guid originKey, Guid targetKey, decimal rate, CancellationToken cancellationToken = default)
    {
        var request = new CurrencyRate
        {
            OriginKey = originKey.ToString(),
            TargetKey = targetKey.ToString(),
            Rate = (double)rate
        };
        var response = await _client.CreateCurrencyRateAsync(request, cancellationToken: cancellationToken);

        return _mapper.Map<CurrencyRateDto>(response);
    }

    public async Task<CurrencyRateDto> DeleteAsync(Guid key, CancellationToken cancellationToken = default)
    {
        var request = new CurrencyRateRequest { Key = key.ToString() };
        var result = await _client.DeleteCurrencyRateAsync(request, cancellationToken: cancellationToken);
        return _mapper.Map<CurrencyRateDto>(result);
    }

    public async Task<CurrencyRateDto> DeleteAsync(Guid originKey, Guid targetKey, CancellationToken cancellationToken = default)
    {
        var conversionResult = await GetConversionAsync(originKey, targetKey, cancellationToken);
        return await DeleteAsync(conversionResult.Key, cancellationToken);
    }

    public async Task<CurrencyRateDto> GetAsync(Guid key, CancellationToken cancellationToken = default)
    {
        var request = new CurrencyRateRequest { Key = key.ToString() };
        var response = await _client.GetCurrencyRateAsync(request, cancellationToken: cancellationToken);
        return _mapper.Map<CurrencyRateDto>(response);
    }

    public async Task<IEnumerable<CurrencyRateDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var response = await _client.GetAllCurrencyRatesAsync(new EmptyRequest(), cancellationToken: cancellationToken);
        return _mapper.Map<IEnumerable<CurrencyRateDto>>(response);
    }

    public async Task<IEnumerable<CurrencyRateDto>> GetAllConversionsAsync(Guid currencyKey, CancellationToken cancellationToken = default)
    {
        var request = new CurrencyRateRequest { Key = currencyKey.ToString() };
        var response = await _client.GetAllCurrencyConversionsAsync(request, cancellationToken: cancellationToken);
        return _mapper.Map<IEnumerable<CurrencyRateDto>>(response);
    }

    public async Task<IEnumerable<CurrencyRateDto>> GetAllRevisionsAsync(Guid key, CancellationToken cancellationToken = default)
    {
        var request = new CurrencyRateRequest { Key = key.ToString() };
        var response = await _client.GetAllCurrencyRateRevisionsAsync(request, cancellationToken: cancellationToken);
        return _mapper.Map<IEnumerable<CurrencyRateDto>>(response);
    }

    public async Task<IEnumerable<CurrencyRateDto>> GetAllRevisionsAsync(Guid originKey, Guid targetKey, CancellationToken cancellationToken = default)
    {
        var conversionResult = await GetConversionAsync(originKey, targetKey, cancellationToken);
        return await GetAllRevisionsAsync(conversionResult.Key, cancellationToken);
    }

    public async Task<CurrencyRateDto> GetConversionAsync(Guid originKey, Guid targetKey, CancellationToken cancellationToken = default)
    {
        var request = new CurrencyConversionRequest
        {
            OriginKey = originKey.ToString(),
            TargetKey = targetKey.ToString()
        };
        var response = await _client.GetCurrencyConversionAsync(request, cancellationToken: cancellationToken);
        return _mapper.Map<CurrencyRateDto>(response);
    }

    public async Task<CurrencyRateDto> GetRevisionAsync(Guid key, DateTime createdAt, CancellationToken cancellationToken = default)
    {
        var request = new CurrencyRateRevisionRequest
        {
            Key = key.ToString(),
            CreatedAt = Timestamp.FromDateTime(createdAt)
        };
        var response = await _client.GetCurrencyRateRevisionAsync(request, cancellationToken: cancellationToken);
        return _mapper.Map<CurrencyRateDto>(response);
    }

    public async Task<CurrencyRateDto> GetRevisionAsync(Guid originKey, Guid targetKey, DateTime createdAt, CancellationToken cancellationToken = default)
    {
        var conversionResult = await GetConversionAsync(originKey, targetKey, cancellationToken);
        return await GetRevisionAsync(conversionResult.Key, createdAt, cancellationToken);
    }

    public async Task<CurrencyRateDto> UpdateAsync(Guid originKey, Guid targetKey, decimal rate, CancellationToken cancellationToken = default)
    {
        var conversionResult = await GetConversionAsync(originKey, targetKey, cancellationToken);
        var request = new CurrencyRateActionRequest
        {
            Key = conversionResult.Key.ToString(),
            CurrencyRate = new CurrencyRate
            {
                OriginKey = originKey.ToString(),
                TargetKey = targetKey.ToString(),
                Rate = (double)rate
            }
        };
        var response = await _client.UpdateCurrencyRateAsync(request, cancellationToken: cancellationToken);
        return _mapper.Map<CurrencyRateDto>(response);
    }

    public async Task<CurrencyRateDto> UpdateAsync(Guid key, decimal rate, CancellationToken cancellationToken = default)
    {
        var currencyRate = await GetAsync(key, cancellationToken);
        var request = new CurrencyRateActionRequest
        {
            Key = key.ToString(),
            CurrencyRate = new CurrencyRate
            {
                OriginKey = currencyRate.Entity.OriginKey.ToString(),
                TargetKey = currencyRate.Entity.TargetKey.ToString(),
                Rate = (double)rate
            }
        };
        var response = await _client.UpdateCurrencyRateAsync(request, cancellationToken: cancellationToken);
        return _mapper.Map<CurrencyRateDto>(response);
    }
}