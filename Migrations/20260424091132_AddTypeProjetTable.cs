using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModuleCRM.Migrations
{
    /// <inheritdoc />
    public partial class AddTypeProjetTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TypesProjet",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeProjetGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Ordre = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TypesProjet", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TypesProjet_TypeProjetGuid",
                table: "TypesProjet",
                column: "TypeProjetGuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TypesProjet_Value",
                table: "TypesProjet",
                column: "Value",
                unique: true);

            // Seed initial des 6 types de projet (GUIDs fixes pour garder la même identité cross-module
            // si la migration est ré-exécutée sur un autre env).
            migrationBuilder.Sql(@"
INSERT INTO TypesProjet (TypeProjetGuid, Value, Label, IsActive, Ordre, CreatedAt, UpdatedAt) VALUES
  ('11111111-1111-1111-1111-000000000001', 'dev',            'Projet Dev',     1, 1, SYSUTCDATETIME(), SYSUTCDATETIME()),
  ('11111111-1111-1111-1111-000000000002', 'helpdesk',       'Helpdesk',       1, 2, SYSUTCDATETIME(), SYSUTCDATETIME()),
  ('11111111-1111-1111-1111-000000000003', 'consulting',     'Consulting',     1, 3, SYSUTCDATETIME(), SYSUTCDATETIME()),
  ('11111111-1111-1111-1111-000000000004', 'formation',      'Formation',      1, 4, SYSUTCDATETIME(), SYSUTCDATETIME()),
  ('11111111-1111-1111-1111-000000000005', 'maintenance',    'Maintenance',    1, 5, SYSUTCDATETIME(), SYSUTCDATETIME()),
  ('11111111-1111-1111-1111-000000000006', 'infrastructure', 'Infrastructure', 1, 6, SYSUTCDATETIME(), SYSUTCDATETIME());
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TypesProjet");
        }
    }
}
