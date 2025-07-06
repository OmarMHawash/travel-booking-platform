using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OmarHawash.TravelBookingPlatform.Migrations
{
    /// <inheritdoc />
    public partial class AddSearchIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add indexes for better search performance
            migrationBuilder.CreateIndex(
                name: "IX_Hotels_Name",
                table: "Hotels",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Cities_Name_Country",
                table: "Cities",
                columns: new[] { "Name", "Country" });

            migrationBuilder.CreateIndex(
                name: "IX_Hotels_Rating",
                table: "Hotels",
                column: "Rating");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_Hotels_Name", table: "Hotels");
            migrationBuilder.DropIndex(name: "IX_Cities_Name_Country", table: "Cities");
            migrationBuilder.DropIndex(name: "IX_Hotels_Rating", table: "Hotels");
        }
    }
}
