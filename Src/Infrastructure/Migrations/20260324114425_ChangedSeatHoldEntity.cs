using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangedSeatHoldEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tickets");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "SeatHolds",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "SeatHolds",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScreeningId = table.Column<int>(type: "int", nullable: false),
                    SeatId = table.Column<int>(type: "int", nullable: false),
                    CineVerseUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PurchasedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tickets_AspNetUsers_CineVerseUserId",
                        column: x => x.CineVerseUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Tickets_Screenings_ScreeningId",
                        column: x => x.ScreeningId,
                        principalTable: "Screenings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tickets_Seats_SeatId",
                        column: x => x.SeatId,
                        principalTable: "Seats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_CineVerseUserId",
                table: "Tickets",
                column: "CineVerseUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ScreeningId_SeatId",
                table: "Tickets",
                columns: new[] { "ScreeningId", "SeatId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_SeatId",
                table: "Tickets",
                column: "SeatId");
        }
    }
}
