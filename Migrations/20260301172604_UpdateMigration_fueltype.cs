using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehicleRent.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMigration_fueltype : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FuelType",
                table: "Vehicles",
                newName: "Fuel");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Fuel",
                table: "Vehicles",
                newName: "FuelType");
        }
    }
}
