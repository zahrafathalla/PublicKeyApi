using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PublicKeyApi.Migrations
{
    /// <inheritdoc />
    public partial class AddPubliKeysTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PublicKey",
                table: "IntegrationClients");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PublicKey",
                table: "IntegrationClients",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
