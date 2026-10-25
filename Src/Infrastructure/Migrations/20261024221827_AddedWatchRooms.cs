using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedWatchRooms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WatchRooms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    HostUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Platform = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    ContentId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ContentUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WatchRooms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WatchRoomMembers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WatchRoomId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WatchRoomMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WatchRoomMembers_WatchRooms_WatchRoomId",
                        column: x => x.WatchRoomId,
                        principalTable: "WatchRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WatchRoomMembers_UserId",
                table: "WatchRoomMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WatchRoomMembers_WatchRoomId_UserId",
                table: "WatchRoomMembers",
                columns: new[] { "WatchRoomId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WatchRooms_Code",
                table: "WatchRooms",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WatchRooms_HostUserId",
                table: "WatchRooms",
                column: "HostUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WatchRoomMembers");

            migrationBuilder.DropTable(
                name: "WatchRooms");
        }
    }
}
