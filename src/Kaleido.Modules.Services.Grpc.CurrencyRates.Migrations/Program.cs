using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Kaleido.Common.Services.Grpc.Configuration.Extensions;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Models;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Configuration;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureAppConfiguration((hostingContext, config) =>
{
    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
    config.AddJsonFile($"appsettings.Development.json", optional: true, reloadOnChange: true);
    config.AddEnvironmentVariables();
});

builder.ConfigureServices((hostContext, services) =>
{
    var connectionString = hostContext.Configuration.GetConnectionString("CurrencyRates");
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new ArgumentNullException(nameof(connectionString), "Expected a value for the currency rates db connection string");
    }
    var assemblyName = "Kaleido.Modules.Services.Grpc.CurrencyRates.Migrations";
    services.AddKaleidoMigrationEntityDbContext<CurrencyRateEntity, CurrencyRateEntityDbContext>(connectionString, assemblyName);
    services.AddKaleidoMigrationRevisionDbContext<BaseRevisionEntity, CurrencyRateRevisionDbContext>(connectionString, assemblyName);

});

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var entityContext = services.GetRequiredService<CurrencyRateEntityDbContext>();
    var revisionContext = services.GetRequiredService<CurrencyRateRevisionDbContext>();

    await entityContext.Database.MigrateAsync();
    await revisionContext.Database.MigrateAsync();

    Console.WriteLine("Migration completed successfully.");
}
