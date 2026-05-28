using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModuleCRM.Migrations
{
    /// <inheritdoc />
    public partial class RefactorOpportunityPhases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Créer la nouvelle table Meetings
            migrationBuilder.CreateTable(
                name: "Meetings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhaseId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Time = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Lieu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Participants = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Done = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Meetings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Meetings_Phases_PhaseId",
                        column: x => x.PhaseId,
                        principalTable: "Phases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_PhaseId",
                table: "Meetings",
                column: "PhaseId");

            // 2) Transférer les MeetingDate/MeetingTime existants vers la table Meetings
            //    (uniquement les phases de type 'meeting' avec une date renseignée)
            migrationBuilder.Sql(@"
                INSERT INTO Meetings (PhaseId, [Date], [Time], Title, Notes, Done, CreatedAt, UpdatedAt)
                SELECT Id, MeetingDate, MeetingTime, NULL, Notes, 0, GETUTCDATE(), GETUTCDATE()
                FROM Phases
                WHERE [Type] = 'meeting' AND MeetingDate IS NOT NULL;
            ");

            // 3) Renommer AgentEtudeId → AgentResponsableId (conservation des données agent)
            migrationBuilder.RenameColumn(
                name: "AgentEtudeId",
                table: "Phases",
                newName: "AgentResponsableId");

            // 4) Ajouter les nouvelles colonnes Document* (vides au départ)
            migrationBuilder.AddColumn<string>(
                name: "DocumentName",
                table: "Phases",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentContentType",
                table: "Phases",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "Phases",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // 5) Initialiser Order selon le Type existant
            migrationBuilder.Sql(@"
                UPDATE Phases SET [Order] = CASE [Type]
                    WHEN 'meeting'     THEN 0
                    WHEN 'proposition' THEN 0
                    WHEN 'study'       THEN 1
                    WHEN 'offer'       THEN 2
                    WHEN 'contract'    THEN 3
                    ELSE 0
                END;
            ");

            // 6) Drop des colonnes mortes
            migrationBuilder.DropColumn(
                name: "MeetingDate",
                table: "Phases");

            migrationBuilder.DropColumn(
                name: "MeetingTime",
                table: "Phases");

            migrationBuilder.DropColumn(
                name: "Reference",
                table: "Phases");

            migrationBuilder.DropColumn(
                name: "DateSignature",
                table: "Phases");

            migrationBuilder.DropColumn(
                name: "Signed",
                table: "Phases");

            // 7) Decimal precision pour Phase.Montant (cohérence avec ValeurEstimee)
            migrationBuilder.AlterColumn<decimal>(
                name: "Montant",
                table: "Phases",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rollback : restaure les colonnes (données non récupérables)
            migrationBuilder.AddColumn<DateTime>(
                name: "MeetingDate",
                table: "Phases",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MeetingTime",
                table: "Phases",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reference",
                table: "Phases",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateSignature",
                table: "Phases",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Signed",
                table: "Phases",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.DropColumn(
                name: "DocumentName",
                table: "Phases");

            migrationBuilder.DropColumn(
                name: "DocumentContentType",
                table: "Phases");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "Phases");

            migrationBuilder.RenameColumn(
                name: "AgentResponsableId",
                table: "Phases",
                newName: "AgentEtudeId");

            migrationBuilder.DropTable(
                name: "Meetings");
        }
    }
}
