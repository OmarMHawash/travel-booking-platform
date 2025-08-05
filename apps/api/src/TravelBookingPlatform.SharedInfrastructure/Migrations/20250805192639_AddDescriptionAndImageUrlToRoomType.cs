using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OmarHawash.TravelBookingPlatform.Migrations
{
    /// <inheritdoc />
    public partial class AddDescriptionAndImageUrlToRoomType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "RoomType",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "RoomType",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "RoomType");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "RoomType");
        }
    }
}
