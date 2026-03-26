using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreatedWebhookEventEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Payments_SeatHoldId",
                table: "Payments");

            migrationBuilder.RenameIndex(
                name: "IX_Tickets_ScreeningId_SeatId",
                table: "Tickets",
                newName: "UX_Tickets_ScreeningId_SeatId");

            migrationBuilder.CreateTable(
                name: "ProcessedWebhookEvent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ProcessedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessedWebhookEvent", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "UX_Payments_SeatHoldId_Pending",
                table: "Payments",
                column: "SeatHoldId",
                unique: true,
                filter: "[Status] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessedWebhookEvent_EventId",
                table: "ProcessedWebhookEvent",
                column: "EventId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProcessedWebhookEvent");

            migrationBuilder.DropIndex(
                name: "UX_Payments_SeatHoldId_Pending",
                table: "Payments");

            migrationBuilder.RenameIndex(
                name: "UX_Tickets_ScreeningId_SeatId",
                table: "Tickets",
                newName: "IX_Tickets_ScreeningId_SeatId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_SeatHoldId",
                table: "Payments",
                column: "SeatHoldId");
        }
    }
}
