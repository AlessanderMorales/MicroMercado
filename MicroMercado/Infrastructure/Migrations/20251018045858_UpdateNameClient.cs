using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MicroMercado.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNameClient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "name",
                table: "clients",
                newName: "business_name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "business_name",
                table: "clients",
                newName: "name");
        }
    }
}
