using Grpc.Net.Client;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Client.Client;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Client.Mapping;
using Microsoft.Extensions.DependencyInjection;
using static Kaleido.Grpc.CurrencyRates.GrpcCurrencyRateService;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Client.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCurrencyRateClient(this IServiceCollection services, string connectionString)
    {
        var channel = GrpcChannel.ForAddress(connectionString);
        var client = new GrpcCurrencyRateServiceClient(channel);
        services.AddSingleton(client);
        services.AddAutoMapper(typeof(CurrencyRateDtoMappingProfile).Assembly);
        services.AddScoped<ICurrencyRateClient, CurrencyRateClient>();

        return services;
    }
}
