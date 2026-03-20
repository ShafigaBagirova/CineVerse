using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedWatchLogEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CineVerseUserId",
                table: "WatchlistItems",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WatchLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MovieId = table.Column<int>(type: "int", nullable: false),
                    CineVerseUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WatchLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WatchLogs_AspNetUsers_CineVerseUserId",
                        column: x => x.CineVerseUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WatchLogs_Movies_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WatchlistItems_CineVerseUserId",
                table: "WatchlistItems",
                column: "CineVerseUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WatchLogs_CineVerseUserId",
                table: "WatchLogs",
                column: "CineVerseUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WatchLogs_MovieId",
                table: "WatchLogs",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_WatchLogs_UserId_MovieId",
                table: "WatchLogs",
                columns: new[] { "UserId", "MovieId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_WatchlistItems_AspNetUsers_CineVerseUserId",
                table: "WatchlistItems",
                column: "CineVerseUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WatchlistItems_AspNetUsers_CineVerseUserId",
                table: "WatchlistItems");

            migrationBuilder.DropTable(
                name: "WatchLogs");

            migrationBuilder.DropIndex(
                name: "IX_WatchlistItems_CineVerseUserId",
                table: "WatchlistItems");

            migrationBuilder.DropColumn(
                name: "CineVerseUserId",
                table: "WatchlistItems");
        }
    }
}
