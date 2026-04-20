using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using RecruitmentAPI.Infrastructure.Data;

#nullable disable

namespace RecruitmentAPI.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(RecruitmentDbContext))]
    [Migration("20260412120000_AddCandidateAddressFields")]
    public partial class AddCandidateAddressFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BuildingNumber",
                table: "Candidates",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "Candidates",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ResidenceDistrictCodeId",
                table: "Candidates",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ResidenceRegionId",
                table: "Candidates",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Street",
                table: "Candidates",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Candidates_ResidenceDistrictCodeId",
                table: "Candidates",
                column: "ResidenceDistrictCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Candidates_ResidenceRegionId",
                table: "Candidates",
                column: "ResidenceRegionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Candidates_DistrictCodes_ResidenceDistrictCodeId",
                table: "Candidates",
                column: "ResidenceDistrictCodeId",
                principalTable: "DistrictCodes",
                principalColumn: "DistrictCodeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Candidates_Regions_ResidenceRegionId",
                table: "Candidates",
                column: "ResidenceRegionId",
                principalTable: "Regions",
                principalColumn: "RegionId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Candidates_DistrictCodes_ResidenceDistrictCodeId",
                table: "Candidates");

            migrationBuilder.DropForeignKey(
                name: "FK_Candidates_Regions_ResidenceRegionId",
                table: "Candidates");

            migrationBuilder.DropIndex(
                name: "IX_Candidates_ResidenceDistrictCodeId",
                table: "Candidates");

            migrationBuilder.DropIndex(
                name: "IX_Candidates_ResidenceRegionId",
                table: "Candidates");

            migrationBuilder.DropColumn(
                name: "BuildingNumber",
                table: "Candidates");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "Candidates");

            migrationBuilder.DropColumn(
                name: "ResidenceDistrictCodeId",
                table: "Candidates");

            migrationBuilder.DropColumn(
                name: "ResidenceRegionId",
                table: "Candidates");

            migrationBuilder.DropColumn(
                name: "Street",
                table: "Candidates");
        }
    }
}
