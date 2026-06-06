using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JwtDemo.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRefreshTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "expired_date",
                table: "refresh_tokens",
                newName: "expiry_date");

            migrationBuilder.RenameColumn(
                name: "added_date",
                table: "refresh_tokens",
                newName: "date_added");

            migrationBuilder.AlterColumn<string>(
                name: "jwt_id",
                table: "refresh_tokens",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "expiry_date",
                table: "refresh_tokens",
                newName: "expired_date");

            migrationBuilder.RenameColumn(
                name: "date_added",
                table: "refresh_tokens",
                newName: "added_date");

            migrationBuilder.AlterColumn<int>(
                name: "jwt_id",
                table: "refresh_tokens",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
