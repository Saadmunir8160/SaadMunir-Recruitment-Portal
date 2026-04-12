using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infra.Migrations
{
    /// <inheritdoc />
    public partial class AddUnitFieldsToDealer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "DealerProducts",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "bags",
                comment: "Unit of measurement for the dealer product (bags or tons)");

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "DealerOrderItems",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "bags",
                comment: "Unit of measurement for the order item (bags or tons)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_DealerProduct_Unit",
                table: "DealerProducts",
                sql: "[Unit] IN ('bags', 'tons')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_DealerOrderItem_Unit",
                table: "DealerOrderItems",
                sql: "[Unit] IN ('bags', 'tons')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_DealerProduct_Unit",
                table: "DealerProducts");

            migrationBuilder.DropCheckConstraint(
                name: "CK_DealerOrderItem_Unit",
                table: "DealerOrderItems");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "DealerProducts");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "DealerOrderItems");
        }
    }
}
