using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedRealationsToFoodAndOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "FoodItems",
                newName: "ImageObjectKey");

            migrationBuilder.AddColumn<int>(
                name: "CinemaId",
                table: "FoodOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CinemaId",
                table: "FoodItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CinemaId",
                table: "FoodCategories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_FoodOrders_CinemaId",
                table: "FoodOrders",
                column: "CinemaId");

            migrationBuilder.CreateIndex(
                name: "IX_FoodItems_CinemaId",
                table: "FoodItems",
                column: "CinemaId");

            migrationBuilder.CreateIndex(
                name: "IX_FoodCategories_CinemaId",
                table: "FoodCategories",
                column: "CinemaId");

            migrationBuilder.AddForeignKey(
                name: "FK_FoodCategories_Cinemas_CinemaId",
                table: "FoodCategories",
                column: "CinemaId",
                principalTable: "Cinemas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FoodItems_Cinemas_CinemaId",
                table: "FoodItems",
                column: "CinemaId",
                principalTable: "Cinemas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FoodOrders_Cinemas_CinemaId",
                table: "FoodOrders",
                column: "CinemaId",
                principalTable: "Cinemas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FoodCategories_Cinemas_CinemaId",
                table: "FoodCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_FoodItems_Cinemas_CinemaId",
                table: "FoodItems");

            migrationBuilder.DropForeignKey(
                name: "FK_FoodOrders_Cinemas_CinemaId",
                table: "FoodOrders");

            migrationBuilder.DropIndex(
                name: "IX_FoodOrders_CinemaId",
                table: "FoodOrders");

            migrationBuilder.DropIndex(
                name: "IX_FoodItems_CinemaId",
                table: "FoodItems");

            migrationBuilder.DropIndex(
                name: "IX_FoodCategories_CinemaId",
                table: "FoodCategories");

            migrationBuilder.DropColumn(
                name: "CinemaId",
                table: "FoodOrders");

            migrationBuilder.DropColumn(
                name: "CinemaId",
                table: "FoodItems");

            migrationBuilder.DropColumn(
                name: "CinemaId",
                table: "FoodCategories");

            migrationBuilder.RenameColumn(
                name: "ImageObjectKey",
                table: "FoodItems",
                newName: "ImageUrl");
        }
    }
}
