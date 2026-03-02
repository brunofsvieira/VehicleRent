using Microsoft.EntityFrameworkCore;
using VehicleRent.Models.Entities;

namespace VehicleRent.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Vehicle> Vehicles { get; set; } = null!;
        public DbSet<Client> Clients { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var vehicle = modelBuilder.Entity<Vehicle>();
            vehicle.HasKey(v => v.Id);
            // Let the database generate the Id (IDENTITY/ValueGeneratedOnAdd)
            vehicle.Property(v => v.Id).ValueGeneratedOnAdd();

            vehicle.Property(v => v.Brand)
                .IsRequired()
                .HasMaxLength(30);

            vehicle.Property(v => v.Model)
                .IsRequired()
                .HasMaxLength(30);

            vehicle.Property(v => v.LicensePlate)
                .IsRequired()
                .HasMaxLength(8);

            vehicle.HasIndex(v => v.LicensePlate)
                .IsUnique();

            vehicle.Property(v => v.Fuel)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(30);

            vehicle.Property(v => v.ManufacturingYear)
                .IsRequired();

            var client = modelBuilder.Entity<Client>();
            client.HasKey(c => c.Id);
            client.Property(c => c.Id).ValueGeneratedOnAdd();

            client.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(50);

            client.Property(c => c.Email)
                .IsRequired()
                .HasMaxLength(100);

            client.HasIndex(c => c.Email)
                .IsUnique();

            client.Property(c => c.PhoneNumber)
                .IsRequired()
                .HasMaxLength(16);

            client.Property(c => c.DriverLicense)
                .IsRequired()
                .HasMaxLength(30);
        }
    }
}
