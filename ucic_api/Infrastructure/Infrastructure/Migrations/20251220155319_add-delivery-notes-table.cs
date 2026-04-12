using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infra.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class adddeliverynotestable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Deliveries",
                columns: table => new
                {
                    DeliveryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LnOrderNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CustomerOrder = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IQN = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    InternalSalesRepresentative = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    QuantityShipped = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    ItemDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DateOut = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateIN = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WeightIN = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    WeightOut = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    ProductionOrder = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Item = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Line = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Shipment = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ShipmentLine = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    WarehouseDescription = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DriverName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Car = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeliveryMeans = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CustomerName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TransporterName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Area = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AreaDescription = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deliveries", x => x.DeliveryID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Deliveries");
        }
    }
}
