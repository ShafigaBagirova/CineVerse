using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreatedMovieEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Movies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AgeRating = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Tagline = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ReleaseDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Director = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    Language = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ImdbRating = table.Column<decimal>(type: "decimal(3,1)", precision: 3, scale: 1, nullable: true),
                    Slug = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    TmdbId = table.Column<long>(type: "bigint", nullable: true),
                    TmdbRating = table.Column<decimal>(type: "decimal(3,1)", precision: 3, scale: 1, nullable: true),
                    UserAverageRating = table.Column<decimal>(type: "decimal(3,1)", precision: 3, scale: 1, nullable: true),
                    RatingCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Movies", x => x.Id);
                    table.CheckConstraint("CK_Movies_DurationMinutes", "[DurationMinutes] > 0");
                });

            migrationBuilder.CreateTable(
                name: "MoviePosters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Order = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ObjectKey = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MovieId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MoviePosters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MoviePosters_Movies_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MoviePosters_MovieId",
                table: "MoviePosters",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_MoviePosters_MovieId_Order",
                table: "MoviePosters",
                columns: new[] { "MovieId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_Movies_ReleaseDate",
                table: "Movies",
                column: "ReleaseDate");

            migrationBuilder.CreateIndex(
                name: "IX_Movies_Slug",
                table: "Movies",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Movies_Status",
                table: "Movies",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Movies_Title",
                table: "Movies",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_Movies_TmdbId",
                table: "Movies",
                column: "TmdbId",
                unique: true,
                filter: "[TmdbId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MoviePosters");

            migrationBuilder.DropTable(
                name: "Movies");
        }
    }
}
