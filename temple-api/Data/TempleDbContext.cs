using Microsoft.EntityFrameworkCore;
using TempleApi.Domain.Entities;

namespace TempleApi.Data
{
    public class TempleDbContext : DbContext
    {
        public TempleDbContext(DbContextOptions<TempleDbContext> options) : base(options)
        {
        }

        public DbSet<Temple> Temples { get; set; }
        public DbSet<Devotee> Devotees { get; set; }
        public DbSet<Donation> Donations { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventRegistration> EventRegistrations { get; set; }
        public DbSet<Service> Services { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Base entity configuration
            modelBuilder.Entity<BaseEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
            });

            // Temple configuration
            modelBuilder.Entity<Temple>(entity =>
            {
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Address).IsRequired().HasMaxLength(200);
                entity.Property(e => e.EstablishedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Devotee configuration
            modelBuilder.Entity<Devotee>(entity =>
            {
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.TempleId).IsRequired();
                
                entity.HasOne(e => e.Temple)
                    .WithMany(e => e.Devotees)
                    .HasForeignKey(e => e.TempleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Donation configuration
            modelBuilder.Entity<Donation>(entity =>
            {
                entity.Property(e => e.TempleId).IsRequired();
                entity.Property(e => e.DonorName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Amount).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.DonationType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Status).HasDefaultValue("Pending");
                entity.Property(e => e.DonationDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                entity.HasOne(e => e.Temple)
                    .WithMany(e => e.Donations)
                    .HasForeignKey(e => e.TempleId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.Devotee)
                    .WithMany(e => e.Donations)
                    .HasForeignKey(e => e.DevoteeId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Event configuration
            modelBuilder.Entity<Event>(entity =>
            {
                entity.Property(e => e.TempleId).IsRequired();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.StartDate).IsRequired();
                entity.Property(e => e.EndDate).IsRequired();
                entity.Property(e => e.EventType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Status).HasDefaultValue("Scheduled");
                
                entity.HasOne(e => e.Temple)
                    .WithMany(e => e.Events)
                    .HasForeignKey(e => e.TempleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // EventRegistration configuration
            modelBuilder.Entity<EventRegistration>(entity =>
            {
                entity.Property(e => e.EventId).IsRequired();
                entity.Property(e => e.DevoteeId).IsRequired();
                entity.Property(e => e.AttendeeName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Status).HasDefaultValue("Registered");
                entity.Property(e => e.RegistrationDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                entity.HasOne(e => e.Event)
                    .WithMany(e => e.Registrations)
                    .HasForeignKey(e => e.EventId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.Devotee)
                    .WithMany(e => e.EventRegistrations)
                    .HasForeignKey(e => e.DevoteeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Service configuration
            modelBuilder.Entity<Service>(entity =>
            {
                entity.Property(e => e.TempleId).IsRequired();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.ServiceType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Price).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.IsAvailable).HasDefaultValue(true);
                
                entity.HasOne(e => e.Temple)
                    .WithMany(e => e.Services)
                    .HasForeignKey(e => e.TempleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            UpdateTimestamps();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries<BaseEntity>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                }
            }
        }
    }
}
