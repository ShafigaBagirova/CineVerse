using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreatedRecommendationNotificationLogEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CineVerseUserId",
                table: "Notifications",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RecommendationNotificationLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    TargetType = table.Column<int>(type: "int", nullable: false),
                    TargetKey = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CineVerseUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecommendationNotificationLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecommendationNotificationLogs_AspNetUsers_CineVerseUserId",
                        column: x => x.CineVerseUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CineVerseUserId",
                table: "Notifications",
                column: "CineVerseUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RecommendationNotificationLogs_CineVerseUserId",
                table: "RecommendationNotificationLogs",
                column: "CineVerseUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RecommendationNotificationLogs_UserId_TargetType_TargetKey",
                table: "RecommendationNotificationLogs",
                columns: new[] { "UserId", "TargetType", "TargetKey" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_AspNetUsers_CineVerseUserId",
                table: "Notifications",
                column: "CineVerseUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_AspNetUsers_CineVerseUserId",
                table: "Notifications");

            migrationBuilder.DropTable(
                name: "RecommendationNotificationLogs");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_CineVerseUserId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "CineVerseUserId",
                table: "Notifications");
        }
    }
}
