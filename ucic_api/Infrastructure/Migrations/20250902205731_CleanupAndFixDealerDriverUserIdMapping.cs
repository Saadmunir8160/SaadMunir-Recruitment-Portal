using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infra.Migrations
{
    /// <inheritdoc />
    public partial class CleanupAndFixDealerDriverUserIdMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Clean up invalid dealer driver records with test user IDs
            migrationBuilder.Sql(@"
                DELETE FROM [DealerDrivers] 
                WHERE [UserID] LIKE 'test-%' 
                   OR [UserID] = 'test-driver-user-id'
                   OR [UserID] NOT IN (SELECT [Id] FROM [auth].[AspNetUsers])");

            // Step 2: Drop the old foreign key constraint that uses ApplicationUserId
            migrationBuilder.DropForeignKey(
                name: "FK_DealerDrivers_AspNetUsers_ApplicationUserId",
                table: "DealerDrivers");

            // Step 3: Drop the old index on ApplicationUserId
            migrationBuilder.DropIndex(
                name: "IX_DealerDrivers_ApplicationUserId",
                table: "DealerDrivers");

            // Step 4: Drop the ApplicationUserId column (it's no longer needed)
            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "DealerDrivers");

            // Step 5: Create index on UserID (the column our entity actually uses)
            migrationBuilder.CreateIndex(
                name: "IX_DealerDrivers_UserID",
                table: "DealerDrivers",
                column: "UserID");

            // Step 6: Create the correct foreign key constraint on UserID
            migrationBuilder.AddForeignKey(
                name: "FK_DealerDrivers_AspNetUsers_UserID",
                table: "DealerDrivers",
                column: "UserID",
                principalSchema: "auth",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverse the changes
            migrationBuilder.DropForeignKey(
                name: "FK_DealerDrivers_AspNetUsers_UserID",
                table: "DealerDrivers");

            migrationBuilder.DropIndex(
                name: "IX_DealerDrivers_UserID",
                table: "DealerDrivers");

            // Re-add the ApplicationUserId column
            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "DealerDrivers",
                type: "nvarchar(450)",
                nullable: true);

            // Re-create the old index and foreign key
            migrationBuilder.CreateIndex(
                name: "IX_DealerDrivers_ApplicationUserId",
                table: "DealerDrivers",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_DealerDrivers_AspNetUsers_ApplicationUserId",
                table: "DealerDrivers",
                column: "ApplicationUserId",
                principalSchema: "auth",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
