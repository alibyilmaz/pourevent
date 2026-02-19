using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pours.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCoveringIndexForSummary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Pours_Summary_Covering",
                table: "Pours",
                columns: new[] { "DeviceId", "StartedAt", "ProductId", "LocationId", "VolumeMl" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Pours_Summary_Covering",
                table: "Pours");
        }
    }
}
