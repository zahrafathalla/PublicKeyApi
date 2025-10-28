using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PublicKeyApi.Migrations
{
    /// <inheritdoc />
    public partial class AddPubliKeysModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IntegrationClientKeys",
                columns: table => new
                {
                    IntegrationClientId = table.Column<int>(type: "int", nullable: false),
                    PublicKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntegrationClientKeys", x => new { x.IntegrationClientId, x.PublicKey });
                    table.ForeignKey(
                        name: "FK_IntegrationClientKeys_IntegrationClients_IntegrationClientId",
                        column: x => x.IntegrationClientId,
                        principalTable: "IntegrationClients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IntegrationClientKeys");
        }
    }
}
