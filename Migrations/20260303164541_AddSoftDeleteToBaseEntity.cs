using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehicleRent.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteToBaseEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Vehicles_LicensePlate",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Clients_DriverLicense",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_Email",
                table: "Clients");

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "Vehicles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "RentalContracts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "Clients",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_LicensePlate",
                table: "Vehicles",
                column: "LicensePlate",
                unique: true,
                filter: "[Deleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_DriverLicense",
                table: "Clients",
                column: "DriverLicense",
                unique: true,
                filter: "[Deleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Email",
                table: "Clients",
                column: "Email",
                unique: true,
                filter: "[Deleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Vehicles_LicensePlate",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Clients_DriverLicense",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_Email",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "RentalContracts");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Clients");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_LicensePlate",
                table: "Vehicles",
                column: "LicensePlate",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_DriverLicense",
                table: "Clients",
                column: "DriverLicense",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Email",
                table: "Clients",
                column: "Email",
                unique: true);
        }
    }
}
