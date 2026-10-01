using DMO.Alpha.Core.Boquilhas;
using DMO.Alpha.Core.Identity;
using DMO.Alpha.Core.Production;
using DMO.Alpha.Core.Tools;
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

    public DbSet<Tool> Tools => Set<Tool>();

    public DbSet<BqRepairTrace> BqRepairTraces => Set<BqRepairTrace>();

    public DbSet<BqMovement> BqMovements => Set<BqMovement>();

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

        // Registo canónico de Tools (Ferramentas). O tool_id é a identidade
        // emitida pelo backend. Não existe nenhuma regra de unicidade sobre
        // factos da Tool (referência, lote, máquina): são apenas atributos de
        // descoberta de candidatos e não formam uma chave de identidade
        // derivada. Um lote diferente persiste como uma Tool distinta com o
        // seu próprio tool_id.
        modelBuilder.Entity<Tool>(entity =>
        {
            entity.HasKey(t => t.ToolId);
            entity.Property(t => t.ToolId).IsRequired();

            // O registo cobre exatamente os tipos actualmente em scope.
            entity.Property(t => t.Type)
                .HasConversion(
                    t => ToolTypeTokens.ToStorage(t),
                    s => ToolTypeTokens.FromStorage(s))
                .IsRequired();

            entity.Property(t => t.Reference).IsRequired();
            entity.Property(t => t.Lot).IsRequired();
        });

        // Registo canónico de reparação de Boquilhas. O trace pertence à
        // Tool física BQ através do tool_id canónico (âncora permanente) e
        // referencia bq_id apenas quando o contexto de produção existe;
        // bq_id não resolvido é null. Não existe estado open/closed nem
        // lifecycle. A cardinalidade de traces pendentes simultâneos por
        // tool_id está deliberadamente em aberto no blueprint: nenhuma
        // regra de unicidade sobre ToolId é imposta aqui. A única unicidade
        // é a relação canónica decidida: um bq_id tem um único trace
        // ("one bq_id has one bq_repair_trace_id"). A associação nunca
        // muta o contexto de produção existente.
        modelBuilder.Entity<BqRepairTrace>(entity =>
        {
            entity.HasKey(t => t.Id);

            entity.HasOne(t => t.Tool)
                .WithMany()
                .HasForeignKey(t => t.ToolId)
                .IsRequired();

            entity.HasOne(t => t.BqContext)
                .WithMany()
                .HasForeignKey(t => t.BqContextId);

            entity.HasIndex(t => t.BqContextId).IsUnique();
        });

        // Movimentos canónicos de Boquilhas. Cada movimento pertence
        // obrigatoriamente a um bq_repair_trace_id; o tipo é exatamente um
        // de saida, entrada, entrada_sem_reparacao; a quantidade observada
        // é registada na totalidade; a discrepância produzida pelo
        // movimento é um facto histórico (null = sem discrepância).
        // Nenhuma regra aritmética é imposta à persistência: sem
        // constraints de sinal, de balanço ou de quantidade — o movimento
        // preserva o que foi fisicamente observado.
        modelBuilder.Entity<BqMovement>(entity =>
        {
            entity.HasKey(m => m.Id);

            entity.HasOne(m => m.Trace)
                .WithMany()
                .HasForeignKey(m => m.BqRepairTraceId)
                .IsRequired();

            entity.Property(m => m.Type)
                .HasConversion(
                    t => BqMovementTypeTokens.ToStorage(t),
                    s => BqMovementTypeTokens.FromStorage(s))
                .IsRequired();

            entity.Property(m => m.Quantity).IsRequired();
        });
    }
}
