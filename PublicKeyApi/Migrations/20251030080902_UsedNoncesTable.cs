using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PublicKeyApi.Migrations
{
    /// <inheritdoc />
    public partial class UsedNoncesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UsedNonces",
                columns: table => new
                {
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsedNonces", x => new { x.ClientId, x.Value });
                    table.ForeignKey(
                        name: "FK_UsedNonces_IntegrationClients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "IntegrationClients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UsedNonces");
        }
    }
}
