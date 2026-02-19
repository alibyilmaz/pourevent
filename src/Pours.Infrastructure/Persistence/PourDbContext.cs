using Microsoft.EntityFrameworkCore;
using Pours.Domain.Entities;

namespace Pours.Infrastructure.Persistence;

public sealed class PourDbContext : DbContext
{
    public PourDbContext(DbContextOptions<PourDbContext> options) : base(options)
    {
    }

    public DbSet<PourEvent> Pours => Set<PourEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PourDbContext).Assembly);
    }
}
