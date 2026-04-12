using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infra.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDealerArea : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AreaID",
                table: "DealerOrders",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DealerAreas",
                columns: table => new
                {
                    AreaID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AreaName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AreaCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DealerAreas", x => x.AreaID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DealerOrders_AreaID",
                table: "DealerOrders",
                column: "AreaID");

            migrationBuilder.AddForeignKey(
                name: "FK_DealerOrders_DealerAreas_AreaID",
                table: "DealerOrders",
                column: "AreaID",
                principalTable: "DealerAreas",
                principalColumn: "AreaID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DealerOrders_DealerAreas_AreaID",
                table: "DealerOrders");

            migrationBuilder.DropTable(
                name: "DealerAreas");

            migrationBuilder.DropIndex(
                name: "IX_DealerOrders_AreaID",
                table: "DealerOrders");

            migrationBuilder.DropColumn(
                name: "AreaID",
                table: "DealerOrders");
        }
    }
}
