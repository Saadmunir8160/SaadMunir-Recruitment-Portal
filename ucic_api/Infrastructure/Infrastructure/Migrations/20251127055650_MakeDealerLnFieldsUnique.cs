using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infra.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeDealerLnFieldsUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_DealerVehicles_Ln_ID",
                table: "DealerVehicles",
                column: "Ln_ID",
                unique: true,
                filter: "[Ln_ID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DealerShippingAddresses_Ln_ID",
                table: "DealerShippingAddresses",
                column: "Ln_ID",
                unique: true,
                filter: "[Ln_ID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Dealers_Ln_ID",
                table: "Dealers",
                column: "Ln_ID",
                unique: true,
                filter: "[Ln_ID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DealerProducts_Product_LnCode",
                table: "DealerProducts",
                column: "Product_LnCode",
                unique: true,
                filter: "[Product_LnCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DealerOrders_Ln_OrderNumber",
                table: "DealerOrders",
                column: "Ln_OrderNumber",
                unique: true,
                filter: "[Ln_OrderNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DealerDrivers_Ln_ID",
                table: "DealerDrivers",
                column: "Ln_ID",
                unique: true,
                filter: "[Ln_ID] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DealerVehicles_Ln_ID",
                table: "DealerVehicles");

            migrationBuilder.DropIndex(
                name: "IX_DealerShippingAddresses_Ln_ID",
                table: "DealerShippingAddresses");

            migrationBuilder.DropIndex(
                name: "IX_Dealers_Ln_ID",
                table: "Dealers");

            migrationBuilder.DropIndex(
                name: "IX_DealerProducts_Product_LnCode",
                table: "DealerProducts");

            migrationBuilder.DropIndex(
                name: "IX_DealerOrders_Ln_OrderNumber",
                table: "DealerOrders");

            migrationBuilder.DropIndex(
                name: "IX_DealerDrivers_Ln_ID",
                table: "DealerDrivers");
        }
    }
}
