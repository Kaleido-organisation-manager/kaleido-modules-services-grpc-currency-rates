using Kaleido.Common.Services.Grpc.Configuration.Extensions;
using Kaleido.Common.Services.Grpc.Handlers.Extensions;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Common.Services.Grpc.Repositories.Extensions;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Configuration;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Mappers;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Models;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Services;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Validators;

var builder = WebApplication.CreateBuilder(args);


//Common
var Configuration = builder.Configuration;
var currencyRatesConnectionString = Configuration.GetConnectionString("CurrencyRates");
if (string.IsNullOrEmpty(currencyRatesConnectionString))
{
    throw new ArgumentNullException(nameof(currencyRatesConnectionString), "No connection string found to connect to the currency rates database");
}

builder.Services.AddAutoMapper(typeof(CurrencyRateMappingProfile));
builder.Services.AddScoped<KeyValidator>();
builder.Services.AddScoped<CurrencyRateValidator>();

builder.Services.AddKaleidoEntityDbContext<CurrencyRateEntity, CurrencyRateEntityDbContext>(currencyRatesConnectionString);
builder.Services.AddKaleidoRevisionDbContext<BaseRevisionEntity, CurrencyRateRevisionDbContext>(currencyRatesConnectionString);

builder.Services.AddEntityRepository<CurrencyRateEntity, CurrencyRateEntityDbContext>();
builder.Services.AddRevisionRepository<BaseRevisionEntity, CurrencyRateRevisionDbContext>();

builder.Services.AddLifeCycleHandler<CurrencyRateEntity, BaseRevisionEntity>();

// Add services to the container.
builder.Services.AddGrpc();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<CurrencyRateService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
