using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModuleCRM.Migrations
{
    /// <inheritdoc />
    public partial class AddDeviseAndPropositionContrat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Devise",
                table: "Opportunities",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PropositionContratContentType",
                table: "Opportunities",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PropositionContratFileName",
                table: "Opportunities",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PropositionContratFilePath",
                table: "Opportunities",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Devise",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "PropositionContratContentType",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "PropositionContratFileName",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "PropositionContratFilePath",
                table: "Opportunities");
        }
    }
}
