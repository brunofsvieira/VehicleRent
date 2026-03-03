using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehicleRent.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMigration_vehicle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LicensePlate",
                table: "Vehicles",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_LicensePlate",
                table: "Vehicles",
                column: "LicensePlate",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Vehicles_LicensePlate",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "LicensePlate",
                table: "Vehicles");
        }
    }
}
