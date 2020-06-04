using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using YounesCo_Backend.Models;

namespace YounesCo_Backend.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        //Creating Roles for our Application

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            //Initialize Roles

            builder.Entity<IdentityRole>().HasData(
                new { Id = "1", Name = "Admin", NormalizedName = "ADMIN" },
                new { Id = "2", Name = "Customer", NormalizedName = "CUSTOMER" },
                new { Id = "3", Name = "Moderator", NormalizedName = "MODERATOR" }
                );

            //Change Location's Colums names

            builder.Entity<AppUser>().OwnsOne(
                u => u.Location,
                l =>
                {
                    l.Property(p => p.Country).HasColumnName("Country");
                    l.Property(p => p.City).HasColumnName("City");
                    l.Property(p => p.Region).HasColumnName("Region");
                    l.Property(p => p.Street).HasColumnName("Street");
                    l.Property(p => p.Building).HasColumnName("Building");
                    l.Property(p => p.Floor).HasColumnName("Floor");

                });

            builder.Entity<OrderItem>(i =>
            {
                i.HasOne(o => o.Product).WithMany(p => p.OrderItems).OnDelete(DeleteBehavior.Restrict);
               // i.HasOne(o => o.Color).WithMany(p => p.OrderItems).OnDelete(DeleteBehavior.Restrict);
                i.Property(o => o.ColorName).HasColumnName("Color");
            });

            builder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(p => p.Orders)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Favorite>()
                .HasKey(f => new { f.CustomerId, f.ProductId });
        }

        public DbSet<Product> Products { get; set; }

        public DbSet<OrderItem> OrderItems { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<Image> Images { get; set; }

        public DbSet<Color> Colors { get; set; }

        public DbSet<Category> Categories { get; set; }
       // public DbSet<Type> Types { get; set; }

        public DbSet<Favorite> Favorites { get; set; }

        public DbSet<ExceptionLog> ExceptionLogs { get; set; }

        public DbSet<Invoice> Invoices { get; set; }

    }
}
