using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangedEmailVerificationCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "EmailVerificationCodes");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "EmailVerificationCodes");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "EmailVerificationCodes");

            migrationBuilder.RenameColumn(
                name: "UserName",
                table: "EmailVerificationCodes",
                newName: "UserId");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "EmailVerificationCodes",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CodeHash",
                table: "EmailVerificationCodes",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_EmailVerificationCodes_Email",
                table: "EmailVerificationCodes",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_EmailVerificationCodes_Email_IsUsed",
                table: "EmailVerificationCodes",
                columns: new[] { "Email", "IsUsed" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailVerificationCodes_ExpiresAtUtc",
                table: "EmailVerificationCodes",
                column: "ExpiresAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmailVerificationCodes_Email",
                table: "EmailVerificationCodes");

            migrationBuilder.DropIndex(
                name: "IX_EmailVerificationCodes_Email_IsUsed",
                table: "EmailVerificationCodes");

            migrationBuilder.DropIndex(
                name: "IX_EmailVerificationCodes_ExpiresAtUtc",
                table: "EmailVerificationCodes");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "EmailVerificationCodes",
                newName: "UserName");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "EmailVerificationCodes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<string>(
                name: "CodeHash",
                table: "EmailVerificationCodes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "EmailVerificationCodes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "EmailVerificationCodes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Gender",
                table: "EmailVerificationCodes",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
