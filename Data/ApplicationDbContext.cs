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
        public DbSet<RentalContract> RentalContracts { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var vehicle = modelBuilder.Entity<Vehicle>();
            vehicle.HasKey(v => v.Id);
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
                .IsUnique()
                .HasFilter("[Deleted] = 0");

            vehicle.Property(v => v.Fuel)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(30);

            vehicle.Property(v => v.ManufacturingYear)
                .IsRequired();
            vehicle.Property(v => v.Deleted).IsRequired();
            vehicle.HasQueryFilter(v => !v.Deleted);

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
                .IsUnique()
                .HasFilter("[Deleted] = 0");

            client.Property(c => c.PhoneNumber)
                .IsRequired()
                .HasMaxLength(16);

            client.Property(c => c.DriverLicense)
                .IsRequired()
                .HasMaxLength(30);
            client.Property(c => c.Deleted).IsRequired();
            client.HasQueryFilter(c => !c.Deleted);

            client.HasIndex(c => c.DriverLicense)
                .IsUnique()
                .HasFilter("[Deleted] = 0");

            var rentalContract = modelBuilder.Entity<RentalContract>();
            rentalContract.HasKey(rc => rc.Id);
            rentalContract.Property(rc => rc.Id).ValueGeneratedOnAdd();

            rentalContract.Property(rc => rc.ClientId).IsRequired();
            rentalContract.Property(rc => rc.VehicleId).IsRequired();
            rentalContract.Property(rc => rc.RentalStartDate).IsRequired();
            rentalContract.Property(rc => rc.RentalEndDate).IsRequired();
            rentalContract.Property(rc => rc.InitialMileage).IsRequired();
            rentalContract.Property(rc => rc.Deleted).IsRequired();
            rentalContract.HasQueryFilter(rc => !rc.Deleted);

            rentalContract.HasOne(rc => rc.Client)
                .WithMany()
                .HasForeignKey(rc => rc.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            rentalContract.HasOne(rc => rc.Vehicle)
                .WithMany()
                .HasForeignKey(rc => rc.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            rentalContract.HasIndex(rc => rc.VehicleId);
            rentalContract.HasIndex(rc => rc.ClientId);
            rentalContract.HasIndex(rc => new { rc.VehicleId, rc.RentalStartDate, rc.RentalEndDate });
        }
    }
}
