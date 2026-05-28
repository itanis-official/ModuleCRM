using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModuleCRM.Migrations
{
    /// <inheritdoc />
    public partial class AddResponsableIdToTaches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ResponsableId",
                table: "ProjetTaches",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ResponsableId",
                table: "ProjetSousTaches",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResponsableId",
                table: "ProjetTaches");

            migrationBuilder.DropColumn(
                name: "ResponsableId",
                table: "ProjetSousTaches");
        }
    }
}
