using Microsoft.EntityFrameworkCore;

namespace PeliculaApi.Infrastructure;

public class RelationalDbContext : DbContext
{
    public DbSet<Entities.PeliculaEntity> Peliculas { get; set; }
    public RelationalDbContext(DbContextOptions<RelationalDbContext> db) : base(db)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Entities.PeliculaEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Director).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ReleaseYear).IsRequired();
            entity.Property(e => e.Genre).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Duration).IsRequired();
        });
    }
}