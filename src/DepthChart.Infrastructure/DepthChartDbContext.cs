using DepthChart.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace DepthChart.Infrastructure;

public class DepthChartDbContext : DbContext
{
    public DepthChartDbContext(DbContextOptions<DepthChartDbContext> options) : base(options) { }

    public DbSet<PlayerEntity> Players => Set<PlayerEntity>();
    public DbSet<TeamEntity> Teams => Set<TeamEntity>();
    public DbSet<DepthChartEntity> DepthCharts => Set<DepthChartEntity>();
    public DbSet<DepthChartPositionEntity> Positions => Set<DepthChartPositionEntity>();
    public DbSet<DepthChartEntryEntity> Entries => Set<DepthChartEntryEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TeamEntity>(eb =>
        {
            eb.ToTable("teams");
            eb.HasKey(x => x.Id);
            eb.Property(x => x.Id).HasColumnName("id");
            eb.Property(x => x.Name).HasColumnName("name");
            eb.Property(x => x.Sport).HasColumnName("sport");
            eb.Property(x => x.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<PlayerEntity>(eb =>
        {
            eb.ToTable("players");
            eb.HasKey(x => x.Id);
            eb.Property(x => x.Id).HasColumnName("id");
            eb.Property(x => x.Name).HasColumnName("name");
            eb.Property(x => x.Number).HasColumnName("number");
            eb.Property(x => x.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<DepthChartEntity>(eb =>
        {
            eb.ToTable("depth_charts");
            eb.HasKey(x => x.Id);
            eb.Property(x => x.Id).HasColumnName("id");
            eb.Property(x => x.TeamId).HasColumnName("team_id");
            eb.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc");

            eb.HasMany(dc => dc.Positions)
              .WithOne(p => p.DepthChart)
              .HasForeignKey(p => p.DepthChartId);
        });

        modelBuilder.Entity<DepthChartPositionEntity>(eb =>
        {
            eb.ToTable("depth_chart_positions"); // 👈 NOT "positions"
            eb.HasKey(x => x.Id);
            eb.Property(x => x.Id).HasColumnName("id");
            eb.Property(x => x.DepthChartId).HasColumnName("depth_chart_id");
            eb.Property(x => x.PositionCode).HasColumnName("position_code");

            eb.HasMany(p => p.Entries)
              .WithOne(e => e.Position)
              .HasForeignKey(e => e.PositionId);
        });

        modelBuilder.Entity<DepthChartEntryEntity>(eb =>
        {
            eb.ToTable("depth_chart_entries"); // 👈 NOT "entries"
            eb.HasKey(x => x.Id);
            eb.Property(x => x.Id).HasColumnName("id");
            eb.Property(x => x.PositionId).HasColumnName("position_id");
            eb.Property(x => x.PlayerId).HasColumnName("player_id");
            eb.Property(x => x.Depth).HasColumnName("depth");
            eb.HasOne(e => e.Player)
              .WithMany() // entries don’t need to hang off Player
              .HasForeignKey(e => e.PlayerId)
              .OnDelete(DeleteBehavior.Cascade);
        });
    }

}
