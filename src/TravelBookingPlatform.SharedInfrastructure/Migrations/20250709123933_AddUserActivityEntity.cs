using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OmarHawash.TravelBookingPlatform.Migrations
{
    /// <inheritdoc />
    public partial class AddUserActivityEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserActivities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    TargetId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TargetType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ActivityDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserActivities_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Note: Deal indexes are excluded from this migration to avoid constraint conflicts

            migrationBuilder.CreateIndex(
                name: "IX_UserActivities_ActivityDate",
                table: "UserActivities",
                column: "ActivityDate");

            migrationBuilder.CreateIndex(
                name: "IX_UserActivities_User_Target_Type",
                table: "UserActivities",
                columns: new[] { "UserId", "TargetId", "TargetType", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_UserActivities_User_TargetType_Date",
                table: "UserActivities",
                columns: new[] { "UserId", "TargetType", "ActivityDate" });

            migrationBuilder.CreateIndex(
                name: "IX_UserActivities_User_Type_Date",
                table: "UserActivities",
                columns: new[] { "UserId", "Type", "ActivityDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserActivities");

            // Note: Deal index drops are excluded from this migration to avoid constraint conflicts
        }
    }
}
