using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehicleRent.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueDriverLicenseForClient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Clients_DriverLicense",
                table: "Clients",
                column: "DriverLicense",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Clients_DriverLicense",
                table: "Clients");
        }
    }
}
