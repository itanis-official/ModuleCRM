using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModuleCRM.Migrations
{
    /// <inheritdoc />
    public partial class AddEquipeIdToOpportunity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EquipeId",
                table: "Opportunities",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EquipeId",
                table: "Opportunities");
        }
    }
}
