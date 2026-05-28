using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModuleCRM.Migrations
{
    /// <inheritdoc />
    public partial class AddMaxHeuresTraitementTicketToCompany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "MaxHeuresTraitementTicket",
                table: "Companies",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxHeuresTraitementTicket",
                table: "Companies");
        }
    }
}
