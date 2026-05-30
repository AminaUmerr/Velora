using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Velora.Migrations
{
    /// <inheritdoc />
    public partial class AddCardFieldsToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CardHolderName",
                table: "Orders",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaskedCardNumber",
                table: "Orders",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CardHolderName",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "MaskedCardNumber",
                table: "Orders");
        }
    }
}
