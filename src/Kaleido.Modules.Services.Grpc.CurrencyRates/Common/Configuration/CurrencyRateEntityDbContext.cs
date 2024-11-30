using Kaleido.Common.Services.Grpc.Configuration.Constants;
using Kaleido.Common.Services.Grpc.Configuration.Interfaces;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Configuration;

public class CurrencyRateEntityDbContext : DbContext, IKaleidoDbContext<CurrencyRateEntity>
{
    public DbSet<CurrencyRateEntity> Items { get; set; } = null!;

    public CurrencyRateEntityDbContext(DbContextOptions<CurrencyRateEntityDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CurrencyRateEntity>(entity =>
        {
            entity.ToTable("CurrencyRates");

            entity.Property(x => x.OriginKey).IsRequired().HasColumnType("varchar(36)");
            entity.Property(x => x.TargetKey).IsRequired().HasColumnType("varchar(36)");
            entity.Property(x => x.Rate).IsRequired().HasColumnType("decimal(18, 2)");

            DefaultOnModelCreatingMethod.ForBaseEntity(entity);
        });
    }
}
