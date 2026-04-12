using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infra.Migrations
{
    /// <inheritdoc />
    public partial class RenameDealerDriverUserIDToUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DealerDrivers_AspNetUsers_UserID",
                table: "DealerDrivers");

            migrationBuilder.DropIndex(
                name: "IX_DealerDrivers_UserID",
                table: "DealerDrivers");

            migrationBuilder.RenameColumn(
                name: "UserID",
                table: "DealerDrivers",
                newName: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "DealerDrivers",
                newName: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_DealerDrivers_UserID",
                table: "DealerDrivers",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_DealerDrivers_AspNetUsers_UserID",
                table: "DealerDrivers",
                column: "UserID",
                principalSchema: "auth",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
