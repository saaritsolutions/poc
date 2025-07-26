using Microsoft.EntityFrameworkCore;
using WorkflowEngine.Core.Entities;

namespace WorkflowEngine.Data;

public class WorkflowDbContext : DbContext
{
    public WorkflowDbContext(DbContextOptions<WorkflowDbContext> options) : base(options)
    {
    }

    public DbSet<Workflow> Workflows { get; set; }
    public DbSet<WorkflowInstance> WorkflowInstances { get; set; }
    public DbSet<WorkflowStep> WorkflowSteps { get; set; }
    public DbSet<WorkflowAuditLog> WorkflowAuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Workflow entity
        modelBuilder.Entity<Workflow>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.WorkflowDefinition).IsRequired();
            entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => new { e.Name, e.Version }).IsUnique();
        });

        // Configure WorkflowInstance entity
        modelBuilder.Entity<WorkflowInstance>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.InstanceData).IsRequired();
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.InitiatedBy).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.InitiatedBy);
            entity.HasIndex(e => e.StartedAt);

            entity.HasOne(e => e.Workflow)
                  .WithMany(w => w.Instances)
                  .HasForeignKey(e => e.WorkflowId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure WorkflowStep entity
        modelBuilder.Entity<WorkflowStep>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.StepId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.StepName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.AssignedTo).HasMaxLength(100);
            entity.Property(e => e.AssignedRole).HasMaxLength(100);
            entity.HasIndex(e => e.AssignedTo);
            entity.HasIndex(e => e.AssignedRole);
            entity.HasIndex(e => e.Status);

            entity.HasOne(e => e.WorkflowInstance)
                  .WithMany(wi => wi.Steps)
                  .HasForeignKey(e => e.WorkflowInstanceId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure WorkflowAuditLog entity
        modelBuilder.Entity<WorkflowAuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PerformedBy).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.PerformedBy);

            entity.HasOne(e => e.WorkflowInstance)
                  .WithMany(wi => wi.AuditLogs)
                  .HasForeignKey(e => e.WorkflowInstanceId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
