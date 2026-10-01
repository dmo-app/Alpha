using DMO.Alpha.Core.Identity;
using DMO.Alpha.Core.Production;
using Microsoft.EntityFrameworkCore;

namespace DMO.Alpha.Infrastructure.Data;

public sealed class DmoDbContext : DbContext
{
    public DmoDbContext(DbContextOptions<DmoDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<AdminAssociation> AdminAssociations => Set<AdminAssociation>();

    public DbSet<Machine> Machines => Set<Machine>();

    public DbSet<JobOn> JobOns => Set<JobOn>();

    public DbSet<CmContext> CmContexts => Set<CmContext>();

    public DbSet<MfContext> MfContexts => Set<MfContext>();

    public DbSet<BqContext> BqContexts => Set<BqContext>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.OperatorId).IsUnique();
            entity.Property(u => u.OperatorId).HasMaxLength(4).IsRequired();
            entity.Property(u => u.Name).IsRequired();
            entity.Property(u => u.ProviderUserId).IsRequired();
            entity.Property(u => u.TemplateName).HasMaxLength(100);
        });

        modelBuilder.Entity<AdminAssociation>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.HasIndex(a => a.Email).IsUnique();
            entity.Property(a => a.Email).IsRequired();
            entity.Property(a => a.ProviderUserId).IsRequired();
        });

        modelBuilder.Entity<Machine>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Code).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<JobOn>(entity =>
        {
            entity.HasKey(j => j.Id);
            entity.Property(j => j.Reference).HasMaxLength(100).IsRequired();
            entity.Property(j => j.ProductionNumber).HasMaxLength(50).IsRequired();
            entity.HasIndex(j => j.ProductionNumber).IsUnique();

            entity.HasOne(j => j.Machine)
                .WithMany()
                .HasForeignKey(j => j.MachineId)
                .IsRequired();

            entity.HasOne(j => j.CmContext)
                .WithOne(c => c.JobOn)
                .HasForeignKey<CmContext>(c => c.JobOnId);

            entity.HasOne(j => j.MfContext)
                .WithOne(c => c.JobOn)
                .HasForeignKey<MfContext>(c => c.JobOnId);

            entity.HasOne(j => j.BqContext)
                .WithOne(c => c.JobOn)
                .HasForeignKey<BqContext>(c => c.JobOnId);
        });

        modelBuilder.Entity<CmContext>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.ToolId).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<MfContext>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.ToolId).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<BqContext>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.ToolId).HasMaxLength(100).IsRequired();
        });
    }
}
