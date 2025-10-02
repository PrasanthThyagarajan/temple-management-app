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
        public DbSet<Area> Areas { get; set; }
        public DbSet<EventType> EventTypes { get; set; }
        public DbSet<EventRegistration> EventRegistrations { get; set; }
        public DbSet<Service> Services { get; set; }
        
        // User Management entities
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<PagePermission> PagePermissions { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        
        // Shop Management entities
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleItem> SaleItems { get; set; }
        public DbSet<Pooja> Poojas { get; set; }
        public DbSet<PoojaBooking> PoojaBookings { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<EventExpense> EventExpenses { get; set; }
        public DbSet<ExpenseService> ExpenseServices { get; set; }
        public DbSet<ExpenseApprovalRoleConfiguration> ExpenseApprovalRoleConfigurations { get; set; }
        public DbSet<EventApprovalRoleConfiguration> EventApprovalRoleConfigurations { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }
        public DbSet<ContributionSetting> ContributionSettings { get; set; }
        public DbSet<Contribution> Contributions { get; set; }
        public DbSet<Inventory> Inventories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Table-per-Type (TPT) inheritance
            modelBuilder.Entity<Temple>().ToTable("Temples");
            modelBuilder.Entity<Devotee>().ToTable("Devotees");
            modelBuilder.Entity<Donation>().ToTable("Donations");
            modelBuilder.Entity<Event>().ToTable("Events");
            modelBuilder.Entity<Area>().ToTable("Areas");
            modelBuilder.Entity<EventType>().ToTable("EventTypes");
            modelBuilder.Entity<EventRegistration>().ToTable("EventRegistrations");
            modelBuilder.Entity<Service>().ToTable("Services");
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Role>().ToTable("Roles");
            modelBuilder.Entity<PagePermission>().ToTable("PagePermissions");
            modelBuilder.Entity<UserRole>().ToTable("UserRoles");
            modelBuilder.Entity<RolePermission>().ToTable("RolePermissions");
            modelBuilder.Entity<Category>().ToTable("Categories");
            modelBuilder.Entity<Product>().ToTable("Products");
            modelBuilder.Entity<Sale>().ToTable("Sales");
            modelBuilder.Entity<SaleItem>().ToTable("SaleItems");
            modelBuilder.Entity<Pooja>().ToTable("Poojas");
            modelBuilder.Entity<PoojaBooking>().ToTable("PoojaBookings");
            modelBuilder.Entity<Expense>().ToTable("Expenses");
            modelBuilder.Entity<ExpenseService>().ToTable("ExpenseServices");
            modelBuilder.Entity<ExpenseApprovalRoleConfiguration>().ToTable("ExpenseApprovalRoleConfigurations");
            modelBuilder.Entity<EventApprovalRoleConfiguration>().ToTable("EventApprovalRoleConfigurations");
            modelBuilder.Entity<ContributionSetting>().ToTable("ContributionSettings");
            modelBuilder.Entity<Contribution>().ToTable("Contributions");
            modelBuilder.Entity<Inventory>().ToTable("Inventories");
            modelBuilder.Entity<EventApprovalRoleConfiguration>(entity =>
            {
                entity.Property(e => e.EventId).IsRequired();
                entity.Property(e => e.UserRoleId).IsRequired();

                entity.HasOne(e => e.Event)
                    .WithMany()
                    .HasForeignKey(e => e.EventId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.UserRole)
                    .WithMany()
                    .HasForeignKey(e => e.UserRoleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ContributionSetting configuration
            modelBuilder.Entity<ContributionSetting>(entity =>
            {
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.ContributionType).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Amount).IsRequired().HasColumnType("decimal(10,2)");
                entity.Property(e => e.RecurringFrequency).HasMaxLength(20);
                
                entity.HasOne(e => e.Event)
                    .WithMany()
                    .HasForeignKey(e => e.EventId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Contribution configuration
            modelBuilder.Entity<Contribution>(entity =>
            {
                entity.Property(e => e.Amount).IsRequired().HasColumnType("decimal(10,2)");
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.PaymentMethod).IsRequired().HasMaxLength(50);
                entity.Property(e => e.TransactionReference).HasMaxLength(100);
                
                entity.HasOne(e => e.Event)
                    .WithMany()
                    .HasForeignKey(e => e.EventId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.Devotee)
                    .WithMany()
                    .HasForeignKey(e => e.DevoteeId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.ContributionSetting)
                    .WithMany()
                    .HasForeignKey(e => e.ContributionSettingId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Inventory configuration
            modelBuilder.Entity<Inventory>(entity =>
            {
                entity.Property(e => e.ItemName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.ItemWorth).IsRequired();
                entity.Property(e => e.ApproximatePrice).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Quantity).IsRequired();
                entity.Property(e => e.Active).IsRequired().HasDefaultValue(true);
                
                entity.HasOne(e => e.Temple)
                    .WithMany()
                    .HasForeignKey(e => e.TempleId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.Area)
                    .WithMany()
                    .HasForeignKey(e => e.AreaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Note: Do not map BaseEntity as a concrete table. Derived entities inherit its properties.

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
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
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
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.StartDate).IsRequired();
                entity.Property(e => e.EndDate).IsRequired();
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.IsApprovalNeeded).HasDefaultValue(false);

                entity.HasOne(e => e.Area)
                    .WithMany(a => a.Events)
                    .HasForeignKey(e => e.AreaId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.EventType)
                    .WithMany(t => t.Events)
                    .HasForeignKey(e => e.EventTypeId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Denormalized for legacy compatibility: keep optional Location text
            });

            // Area configuration
            modelBuilder.Entity<Area>(entity =>
            {
                entity.Property(a => a.TempleId).IsRequired();
                entity.Property(a => a.Name).IsRequired().HasMaxLength(200);

                entity.HasOne(a => a.Temple)
                    .WithMany(t => t.Areas)
                    .HasForeignKey(a => a.TempleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // EventType configuration
            modelBuilder.Entity<EventType>(entity =>
            {
                entity.Property(t => t.Name).IsRequired().HasMaxLength(100);
                entity.HasIndex(t => t.Name).IsUnique();
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

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.Gender).HasMaxLength(20);
                entity.Property(e => e.Address).HasMaxLength(200);
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Role configuration
            modelBuilder.Entity<Role>(entity =>
            {
                entity.Property(e => e.RoleName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(200);
                
                entity.HasIndex(e => e.RoleName).IsUnique();
            });

            // PagePermission configuration
            modelBuilder.Entity<PagePermission>(entity =>
            {
                entity.Property(e => e.PageName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PageUrl).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PermissionId).IsRequired();
                
                // Create composite unique constraint on PageName + PermissionId
                entity.HasIndex(e => new { e.PageName, e.PermissionId }).IsUnique();
                entity.Ignore(e => e.Permission); // Ignore the computed property
            });

            // UserRole configuration
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(e => e.UserRoleId);
                
                entity.HasOne(e => e.User)
                    .WithMany(e => e.UserRoles)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.Role)
                    .WithMany(e => e.UserRoles)
                    .HasForeignKey(e => e.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasIndex(e => new { e.UserId, e.RoleId }).IsUnique();
            });

            // RolePermission configuration
            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.HasKey(e => e.RolePermissionId);
                
                entity.HasOne(e => e.Role)
                    .WithMany(e => e.RolePermissions)
                    .HasForeignKey(e => e.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.PagePermission)
                    .WithMany(e => e.RolePermissions)
                    .HasForeignKey(e => e.PagePermissionId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasIndex(e => new { e.RoleId, e.PagePermissionId }).IsUnique();
            });

            // Category configuration
            modelBuilder.Entity<Category>(entity =>
            {
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.SortOrder).HasDefaultValue(0);
                
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // Product configuration
            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Category).HasMaxLength(50);
                entity.Property(e => e.Price).IsRequired().HasColumnType("decimal(10,2)");
                entity.Property(e => e.Quantity).HasDefaultValue(0);
                entity.Property(e => e.MinStockLevel).HasDefaultValue(0);
                entity.Property(e => e.Description).HasColumnType("TEXT");
                entity.Property(e => e.Notes).HasColumnType("TEXT");
                
                entity.HasOne(e => e.CategoryNavigation)
                    .WithMany(e => e.Products)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Sale configuration
            modelBuilder.Entity<Sale>(entity =>
            {
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.StaffId).IsRequired();
                entity.Property(e => e.TotalAmount).IsRequired().HasColumnType("decimal(10,2)");
                entity.Property(e => e.DiscountAmount).HasColumnType("decimal(10,2)");
                entity.Property(e => e.FinalAmount).HasColumnType("decimal(10,2)");
                entity.Property(e => e.PaymentMethod).HasMaxLength(50);
                entity.Property(e => e.SaleDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.Notes).HasColumnType("TEXT");
                
                // Note: User navigation properties will be added when needed
                // entity.HasOne(e => e.Customer)
                //     .WithMany()
                //     .HasForeignKey(e => e.UserId)
                //     .OnDelete(DeleteBehavior.Restrict);
                
                // entity.HasOne(e => e.Staff)
                //     .WithMany()
                //     .HasForeignKey(e => e.StaffId)
                //     .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Event)
                    .WithMany()
                    .HasForeignKey(e => e.EventId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // SaleItem configuration
            modelBuilder.Entity<SaleItem>(entity =>
            {
                entity.Property(e => e.SaleId).IsRequired();
                entity.Property(e => e.ProductId).IsRequired();
                entity.Property(e => e.Quantity).IsRequired();
                entity.Property(e => e.UnitPrice).IsRequired().HasColumnType("decimal(10,2)");
                entity.Property(e => e.Subtotal).IsRequired().HasColumnType("decimal(10,2)");
                
                entity.HasOne(e => e.Sale)
                    .WithMany(e => e.SaleItems)
                    .HasForeignKey(e => e.SaleId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.Product)
                    .WithMany(e => e.SaleItems)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Pooja configuration
            modelBuilder.Entity<Pooja>(entity =>
            {
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasColumnType("TEXT");
                entity.Property(e => e.Price).IsRequired().HasColumnType("decimal(10,2)");
            });

            // PoojaBooking configuration
            modelBuilder.Entity<PoojaBooking>(entity =>
            {
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.PoojaId).IsRequired();
                entity.Property(e => e.ScheduledDate).IsRequired();
                entity.Property(e => e.Amount).IsRequired().HasColumnType("decimal(10,2)");
                entity.Property(e => e.Status).IsRequired();
                entity.Property(e => e.BookingDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                // Note: User navigation properties will be added when needed
                // entity.HasOne(e => e.Customer)
                //     .WithMany()
                //     .HasForeignKey(e => e.UserId)
                //     .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.Pooja)
                    .WithMany(e => e.Bookings)
                    .HasForeignKey(e => e.PoojaId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                // entity.HasOne(e => e.Staff)
                //     .WithMany()
                //     .HasForeignKey(e => e.StaffId)
                //     .OnDelete(DeleteBehavior.SetNull);
            });

        // EventExpense configuration
        modelBuilder.Entity<EventExpense>(entity =>
        {
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsApprovalNeeded).HasDefaultValue(false);

            entity.HasOne(e => e.ApprovalRole)
                .WithMany()
                .HasForeignKey(e => e.ApprovalRoleId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ExpenseService configuration
        modelBuilder.Entity<ExpenseService>(entity =>
        {
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsApprovalNeeded).HasDefaultValue(false);

            entity.HasOne(e => e.ApprovalRole)
                .WithMany()
                .HasForeignKey(e => e.ApprovalRoleId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // EventExpense (Expense) configuration
        modelBuilder.Entity<Expense>(entity =>
        {
            // Either EventExpenseId or ExpenseServiceId should be present

            entity.HasOne(e => e.EventExpense)
                .WithMany()
                .HasForeignKey(e => e.EventExpenseId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.ExpenseService)
                .WithMany()
                .HasForeignKey(e => e.ExpenseServiceId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Make EventId required foreign key for Event Expenses
            entity.Property(e => e.EventId).IsRequired();
            entity.HasOne(e => e.Event)
                .WithMany()
                .HasForeignKey(e => e.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // Approval configuration
            entity.Property(e => e.IsApprovalNeed).HasDefaultValue(false);
            entity.Property(e => e.IsApproved).HasDefaultValue(false);
            entity.HasOne(e => e.RequestedByUser)
                .WithMany()
                .HasForeignKey(e => e.RequestedBy)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.ApprovedByUser)
                .WithMany()
                .HasForeignKey(e => e.ApprovedBy)
                .OnDelete(DeleteBehavior.SetNull);
        });

            // ExpenseApprovalRoleConfiguration configuration
            modelBuilder.Entity<ExpenseApprovalRoleConfiguration>(entity =>
            {
                entity.Property(e => e.UserRoleId).IsRequired();
                entity.Property(e => e.EventExpenseId).IsRequired();

                entity.HasOne(e => e.UserRole)
                    .WithMany()
                    .HasForeignKey(e => e.UserRoleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.EventExpense)
                    .WithMany()
                    .HasForeignKey(e => e.EventExpenseId)
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
