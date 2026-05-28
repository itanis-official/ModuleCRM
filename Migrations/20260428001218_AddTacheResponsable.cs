using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModuleCRM.Migrations
{
    /// <inheritdoc />
    public partial class AddTacheResponsable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResponsableNom",
                table: "ProjetTaches",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResponsableNom",
                table: "ProjetTaches");
        }
    }
}
