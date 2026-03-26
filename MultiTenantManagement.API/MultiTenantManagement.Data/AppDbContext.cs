using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MultiTenantManagement.Core.Interfaces;
using MultiTenantManagement.Data.Models;

namespace MultiTenantManagement.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly ICurrentUserContext _currentUser;

        public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserContext currentUser)
           : base(options)
        {
            _currentUser = currentUser;
        }

        public DbSet<Tenant> Tenants => Set<Tenant>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Attachment> Attachments => Set<Attachment>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItems> OrderItems => Set<OrderItems>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();
        public DbSet<StoreSetting> StoreSettings => Set<StoreSetting>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
           //builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            builder.Entity<Tenant>(e =>
            {
                e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            });

            builder.Entity<ApplicationUser>(e =>
            {
                e.HasIndex(x => x.TenantId);

                e.HasOne(u => u.Tenant)
                 .WithMany(t => t.Users)
                 .HasForeignKey(u => u.TenantId)
                 .OnDelete(DeleteBehavior.Restrict)
                 .IsRequired(false);
            });

            builder.Entity<Product>(e =>
            {
                e.HasOne(p => p.Tenant)
                 .WithMany(t => t.Products)
                 .HasForeignKey(p => p.TenantId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(p => p.Attachment)
                 .WithMany()
                 .HasForeignKey(p => p.AttachmentId)
                 .OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<Order>(e =>
            {
                e.HasKey(x => x.Id);

                e.Property(x => x.DeliveryAddress)
                    .IsRequired()
                    .HasMaxLength(500);

                e.Property(x => x.TotalAmount)
                    .HasColumnType("decimal(18,2)");

                e.Property(x => x.Status)
                    .IsRequired();

                e.Property(x => x.CreatedAtUtc)
                    .IsRequired();

                e.HasIndex(x => x.TenantId);
                e.HasIndex(x => x.CustomerId);

                e.HasOne(x => x.Tenant)
                    .WithMany()
                    .HasForeignKey(x => x.TenantId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasMany(x => x.Items)
                    .WithOne()
                    .HasForeignKey("OrderId")
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasMany(x => x.StatusHistory)
                    .WithOne()
                    .HasForeignKey("OrderId")
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Payment)
                    .WithOne()
                    .HasForeignKey<Payment>("OrderId")
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<OrderItems>(e =>
            {
                e.HasKey(x => x.Id);

                e.Property(x => x.Quantity)
                    .IsRequired();

                e.Property(x => x.UnitPrice)
                    .HasColumnType("decimal(18,2)");

                e.HasIndex(x => x.TenantId);
                e.HasIndex(x => x.OrderId);
                e.HasIndex(x => x.ProductId);

                e.HasOne(x => x.Order)
                    .WithMany(x => x.Items)
                    .HasForeignKey(x => x.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Product)
                    .WithMany()
                    .HasForeignKey(x => x.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<OrderStatusHistory>(e =>
            {
                e.HasKey(x => x.Id);

                e.Property(x => x.FromStatus)
                    .IsRequired();

                e.Property(x => x.ToStatus)
                    .IsRequired();

                e.Property(x => x.ActionName)
                    .IsRequired()
                    .HasMaxLength(200);

                e.Property(x => x.Comment)
                    .HasMaxLength(1000);

                e.Property(x => x.ChangedBy)
                    .IsRequired();

                e.Property(x => x.ChangedAtUtc)
                    .IsRequired();

                e.HasIndex(x => x.TenantId);
                e.HasIndex(x => x.OrderId);

                e.HasOne(x => x.Order)
                    .WithMany(x => x.StatusHistory)
                    .HasForeignKey(x => x.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Payment>(e =>
            {
                e.Property(x => x.TenantId).IsRequired();
                e.HasIndex(x => x.TenantId);
            });

            builder.Entity<StoreSetting>(e =>
            {
                e.HasOne(s => s.Tenant)
                 .WithOne(t => t.StoreSetting)
                 .HasForeignKey<StoreSetting>(s => s.TenantId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Tenant>(e =>
            {
                e.HasOne(t => t.Attachment)
                 .WithMany()
                 .HasForeignKey(t => t.AttachmentId)
                 .OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<Attachment>(e =>
            {
                e.Property(x => x.OriginalFileName).HasMaxLength(255).IsRequired();
                e.Property(x => x.TenantId).IsRequired();
                e.Property(x => x.StoredFileName).HasMaxLength(255).IsRequired();
                e.Property(x => x.FileKey).HasMaxLength(1024).IsRequired();
                e.Property(x => x.StorageProvider).HasMaxLength(50).IsRequired();
                e.Property(x => x.ContentType).HasMaxLength(150).IsRequired();
                e.Property(x => x.Category).HasMaxLength(100).IsRequired();
                e.Property(x => x.EntityType).HasMaxLength(100).IsRequired();
                e.Property(x => x.EntityId).HasMaxLength(100).IsRequired();
                e.HasIndex(x => x.TenantId);
            });

            // Global query filters can be re-enabled when current user tenant scoping is required globally.
        }
    }
}
