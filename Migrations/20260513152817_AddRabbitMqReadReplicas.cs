using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModuleCRM.Migrations
{
    /// <inheritdoc />
    public partial class AddRabbitMqReadReplicas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AgentsLocal",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    AgentType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Nom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Prenom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Telephone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Role = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Poste = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Departement = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CoutHoraire = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Rating = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    SyncedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentsLocal", x => new { x.Id, x.AgentType });
                });

            migrationBuilder.CreateTable(
                name: "EquipesLocal",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EquipeGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceModule = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdOrigine = table.Column<int>(type: "int", nullable: false),
                    Nom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Domaine = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChefProjetIdOrigine = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SyncedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipesLocal", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EquipesMembresLocal",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EquipeLocalId = table.Column<int>(type: "int", nullable: false),
                    CollaborateurIdOrigine = table.Column<int>(type: "int", nullable: false),
                    CollaborateurType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RoleDansEquipe = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateAffectation = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipesMembresLocal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EquipesMembresLocal_EquipesLocal_EquipeLocalId",
                        column: x => x.EquipeLocalId,
                        principalTable: "EquipesLocal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Projets_ClientId",
                table: "Projets",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentsLocal_Departement",
                table: "AgentsLocal",
                column: "Departement");

            migrationBuilder.CreateIndex(
                name: "IX_AgentsLocal_Email",
                table: "AgentsLocal",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_AgentsLocal_Role",
                table: "AgentsLocal",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_EquipesLocal_EquipeGuid",
                table: "EquipesLocal",
                column: "EquipeGuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EquipesMembresLocal_EquipeLocalId",
                table: "EquipesMembresLocal",
                column: "EquipeLocalId");

            migrationBuilder.AddForeignKey(
                name: "FK_Projets_Companies_ClientId",
                table: "Projets",
                column: "ClientId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projets_Companies_ClientId",
                table: "Projets");

            migrationBuilder.DropTable(
                name: "AgentsLocal");

            migrationBuilder.DropTable(
                name: "EquipesMembresLocal");

            migrationBuilder.DropTable(
                name: "EquipesLocal");

            migrationBuilder.DropIndex(
                name: "IX_Projets_ClientId",
                table: "Projets");
        }
    }
}
