using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModuleCRM.Migrations
{
    /// <inheritdoc />
    public partial class AddProjetHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProjetPhases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjetId = table.Column<int>(type: "int", nullable: false),
                    TypePhase = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ordre = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjetPhases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjetPhases_Projets_ProjetId",
                        column: x => x.ProjetId,
                        principalTable: "Projets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjetTaches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjetPhaseId = table.Column<int>(type: "int", nullable: false),
                    Titre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Statut = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ordre = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjetTaches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjetTaches_ProjetPhases_ProjetPhaseId",
                        column: x => x.ProjetPhaseId,
                        principalTable: "ProjetPhases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjetSousTaches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjetTacheId = table.Column<int>(type: "int", nullable: false),
                    Titre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Statut = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DureeEstimeeHeures = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    ResponsableNom = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ordre = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjetSousTaches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjetSousTaches_ProjetTaches_ProjetTacheId",
                        column: x => x.ProjetTacheId,
                        principalTable: "ProjetTaches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjetPhases_ProjetId",
                table: "ProjetPhases",
                column: "ProjetId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjetSousTaches_ProjetTacheId",
                table: "ProjetSousTaches",
                column: "ProjetTacheId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjetTaches_ProjetPhaseId",
                table: "ProjetTaches",
                column: "ProjetPhaseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjetSousTaches");

            migrationBuilder.DropTable(
                name: "ProjetTaches");

            migrationBuilder.DropTable(
                name: "ProjetPhases");
        }
    }
}
