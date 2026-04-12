using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infra.Migrations
{
    /// <inheritdoc />
    public partial class updatjobtable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "DepartmentId",
                table: "Jobs",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<bool>(
                name: "IsArabic",
                table: "Jobs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "JobsId",
                table: "Department",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Department_JobsId",
                table: "Department",
                column: "JobsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Department_Jobs_JobsId",
                table: "Department",
                column: "JobsId",
                principalTable: "Jobs",
                principalColumn: "JobsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Department_Jobs_JobsId",
                table: "Department");

            migrationBuilder.DropIndex(
                name: "IX_Department_JobsId",
                table: "Department");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "IsArabic",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "JobsId",
                table: "Department");
        }
    }
}
