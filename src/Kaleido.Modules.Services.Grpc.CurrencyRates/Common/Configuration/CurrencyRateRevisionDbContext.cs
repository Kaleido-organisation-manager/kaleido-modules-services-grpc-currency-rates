using Kaleido.Common.Services.Grpc.Configuration.Constants;
using Kaleido.Common.Services.Grpc.Configuration.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Microsoft.EntityFrameworkCore;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Configuration;

public class CurrencyRateRevisionDbContext : DbContext, IKaleidoDbContext<BaseRevisionEntity>
{
    public DbSet<BaseRevisionEntity> Items { get; set; } = null!;

    public CurrencyRateRevisionDbContext(DbContextOptions<CurrencyRateRevisionDbContext> options)
    : base(options)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<BaseRevisionEntity>(entity =>
        {
            entity.ToTable("CurrencyRateRevisions");

            DefaultOnModelCreatingMethod.ForBaseRevisionEntity(entity);
        });
    }
}