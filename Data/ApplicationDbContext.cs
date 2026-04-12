using Microsoft.EntityFrameworkCore;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Project> Projects => Set<Project>();
        public DbSet<Milestone> Milestones => Set<Milestone>();
        public DbSet<WorkItem> WorkItems => Set<WorkItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Project>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.StartDate).IsRequired();
                entity.Property(e => e.TargetEndDate).IsRequired();
                entity.HasMany(e => e.Milestones)
                    .WithOne(m => m.Project)
                    .HasForeignKey(m => m.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.WorkItems)
                    .WithOne(w => w.Project)
                    .HasForeignKey(w => w.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Milestone>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.ScheduledDate).IsRequired();
                entity.Property(e => e.Status)
                    .HasConversion<string>()
                    .HasMaxLength(50);
                entity.Property(e => e.CompletionPercentage)
                    .HasPrecision(5, 2);
                entity.HasIndex(e => e.ProjectId);
                entity.HasIndex(e => e.Status);
                entity.HasMany(e => e.WorkItems)
                    .WithOne(w => w.Milestone)
                    .HasForeignKey(w => w.MilestoneId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<WorkItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Status)
                    .HasConversion<string>()
                    .HasMaxLength(50);
                entity.Property(e => e.CreatedDate).IsRequired();
                entity.Property(e => e.OwnerName).HasMaxLength(255);
                entity.HasIndex(e => e.ProjectId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.MilestoneId);
            });
        }
    }
}