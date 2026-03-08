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
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItems> OrderItems => Set<OrderItems>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<StoreSetting> StoreSettings => Set<StoreSetting>();


        protected override void OnModelCreating(ModelBuilder builder)
        {
                base.OnModelCreating(builder);

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
                });

                builder.Entity<Order>(e =>
                {
                    e.HasOne(o => o.Tenant)
                     .WithMany(t => t.Orders)
                     .HasForeignKey(o => o.TenantId)
                     .OnDelete(DeleteBehavior.Restrict);

                    e.HasMany(o => o.Items)
                     .WithOne(i => i.Order)
                     .HasForeignKey(i => i.OrderId)
                     .OnDelete(DeleteBehavior.Cascade);

                    e.HasOne(o => o.Payment)
                     .WithOne(p => p.Order)
                     .HasForeignKey<Payment>(p => p.OrderId)
                     .OnDelete(DeleteBehavior.Cascade);
                });

                builder.Entity<OrderItems>(e =>
                {
                    e.HasOne(i => i.Product)
                     .WithMany(p => p.OrderItems)
                     .HasForeignKey(i => i.ProductId)
                     .OnDelete(DeleteBehavior.Restrict);
                });

                builder.Entity<StoreSetting>(e =>
                {
                    e.HasOne(s => s.Tenant)
                     .WithOne(t => t.StoreSetting)
                     .HasForeignKey<StoreSetting>(s => s.TenantId)
                     .OnDelete(DeleteBehavior.Cascade);
                });



            //Global Query Filter
            //builder.Entity<Product>()
            //   .HasQueryFilter(p =>
            //       _currentUser.IsSuperAdmin ||
            //       (_currentUser.TenantId != null && p.TenantId == _currentUser.TenantId)
            //   );

            //builder.Entity<Order>()
            //  .HasQueryFilter(p =>
            //      _currentUser.IsSuperAdmin ||
            //      (_currentUser.TenantId != null && p.TenantId == _currentUser.TenantId)
            //  );

            //builder.Entity<Tenant>()
            //    .HasQueryFilter(p =>
            //        _currentUser.IsSuperAdmin ||
            //        (_currentUser.TenantId != null && p.Id == _currentUser.TenantId)
            //     );

        }

    }
}
