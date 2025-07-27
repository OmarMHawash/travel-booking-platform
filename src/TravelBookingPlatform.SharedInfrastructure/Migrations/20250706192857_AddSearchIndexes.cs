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
            // Note: IX_Hotel_Rating already exists from AddHotelBookingEntities migration
            // Only add new indexes that don't already exist
            migrationBuilder.CreateIndex(
                name: "IX_City_Name_Country",
                table: "City",
                columns: new[] { "Name", "Country" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_City_Name_Country", table: "City");
        }
    }
}
