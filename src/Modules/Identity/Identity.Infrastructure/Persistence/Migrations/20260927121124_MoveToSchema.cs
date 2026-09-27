using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Diaspora.Identity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MoveToSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "identity");

            migrationBuilder.RenameTable(
                name: "users",
                newName: "users",
                newSchema: "identity");

            migrationBuilder.RenameTable(
                name: "refresh_tokens",
                newName: "refresh_tokens",
                newSchema: "identity");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "users",
                schema: "identity",
                newName: "users");

            migrationBuilder.RenameTable(
                name: "refresh_tokens",
                schema: "identity",
                newName: "refresh_tokens");
        }
    }
}
