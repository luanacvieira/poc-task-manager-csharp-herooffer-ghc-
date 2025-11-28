using Microsoft.EntityFrameworkCore;

namespace TaskManager.Web.Data;

/// <summary>
/// Contexto de banco de dados para o Task Manager
/// </summary>
public class TaskManagerDbContext : DbContext
{
    public TaskManagerDbContext(DbContextOptions<TaskManagerDbContext> options)
        : base(options)
    {
    }

    public DbSet<Models.TaskItem> Tasks { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Models.TaskItem>(entity =>
        {
            entity.ToTable("Tasks");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Description)
                .HasMaxLength(2000);

            entity.Property(e => e.Priority)
                .IsRequired()
                .HasConversion<string>();

            entity.Property(e => e.Category)
                .IsRequired()
                .HasConversion<string>();

            entity.Property(e => e.DueDate);

            // Tags como JSON no SQL Server
            entity.Property(e => e.Tags)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                );

            entity.Property(e => e.AssignedTo)
                .HasMaxLength(100);

            entity.Property(e => e.UserId)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Completed)
                .IsRequired();

            // Campos de auditoria
            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.UpdatedAt)
                .IsRequired();

            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100);

            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(100);

            // Controle de concorrência otimista
            entity.Property(e => e.RowVersion)
                .IsRowVersion();

            // Índices para performance
            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("IX_Tasks_UserId");

            entity.HasIndex(e => e.Completed)
                .HasDatabaseName("IX_Tasks_Completed");

            entity.HasIndex(e => e.Priority)
                .HasDatabaseName("IX_Tasks_Priority");

            entity.HasIndex(e => e.Category)
                .HasDatabaseName("IX_Tasks_Category");

            entity.HasIndex(e => e.DueDate)
                .HasDatabaseName("IX_Tasks_DueDate");

            entity.HasIndex(e => new { e.UserId, e.Completed })
                .HasDatabaseName("IX_Tasks_UserId_Completed");

            entity.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("IX_Tasks_CreatedAt");
        });
    }
}
