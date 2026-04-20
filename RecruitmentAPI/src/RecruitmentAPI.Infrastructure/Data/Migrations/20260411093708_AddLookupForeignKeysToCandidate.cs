using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecruitmentAPI.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLookupForeignKeysToCandidate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NationalityId",
                table: "Candidates",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ResidenceCityId",
                table: "Candidates",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ResidenceCountryId",
                table: "Candidates",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ResidenceDistrictId",
                table: "Candidates",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CountryId",
                table: "CandidateExperiences",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrencyId",
                table: "CandidateExperiences",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Candidates_NationalityId",
                table: "Candidates",
                column: "NationalityId");

            migrationBuilder.CreateIndex(
                name: "IX_Candidates_ResidenceCityId",
                table: "Candidates",
                column: "ResidenceCityId");

            migrationBuilder.CreateIndex(
                name: "IX_Candidates_ResidenceCountryId",
                table: "Candidates",
                column: "ResidenceCountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Candidates_ResidenceDistrictId",
                table: "Candidates",
                column: "ResidenceDistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateExperiences_CountryId",
                table: "CandidateExperiences",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateExperiences_CurrencyId",
                table: "CandidateExperiences",
                column: "CurrencyId");

            migrationBuilder.AddForeignKey(
                name: "FK_CandidateExperiences_Countries_CountryId",
                table: "CandidateExperiences",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "CountryId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CandidateExperiences_Currencies_CurrencyId",
                table: "CandidateExperiences",
                column: "CurrencyId",
                principalTable: "Currencies",
                principalColumn: "CurrencyId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Candidates_Cities_ResidenceCityId",
                table: "Candidates",
                column: "ResidenceCityId",
                principalTable: "Cities",
                principalColumn: "CityId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Candidates_Countries_ResidenceCountryId",
                table: "Candidates",
                column: "ResidenceCountryId",
                principalTable: "Countries",
                principalColumn: "CountryId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Candidates_Districts_ResidenceDistrictId",
                table: "Candidates",
                column: "ResidenceDistrictId",
                principalTable: "Districts",
                principalColumn: "DistrictId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Candidates_Nationalities_NationalityId",
                table: "Candidates",
                column: "NationalityId",
                principalTable: "Nationalities",
                principalColumn: "NationalityId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CandidateExperiences_Countries_CountryId",
                table: "CandidateExperiences");

            migrationBuilder.DropForeignKey(
                name: "FK_CandidateExperiences_Currencies_CurrencyId",
                table: "CandidateExperiences");

            migrationBuilder.DropForeignKey(
                name: "FK_Candidates_Cities_ResidenceCityId",
                table: "Candidates");

            migrationBuilder.DropForeignKey(
                name: "FK_Candidates_Countries_ResidenceCountryId",
                table: "Candidates");

            migrationBuilder.DropForeignKey(
                name: "FK_Candidates_Districts_ResidenceDistrictId",
                table: "Candidates");

            migrationBuilder.DropForeignKey(
                name: "FK_Candidates_Nationalities_NationalityId",
                table: "Candidates");

            migrationBuilder.DropIndex(
                name: "IX_Candidates_NationalityId",
                table: "Candidates");

            migrationBuilder.DropIndex(
                name: "IX_Candidates_ResidenceCityId",
                table: "Candidates");

            migrationBuilder.DropIndex(
                name: "IX_Candidates_ResidenceCountryId",
                table: "Candidates");

            migrationBuilder.DropIndex(
                name: "IX_Candidates_ResidenceDistrictId",
                table: "Candidates");

            migrationBuilder.DropIndex(
                name: "IX_CandidateExperiences_CountryId",
                table: "CandidateExperiences");

            migrationBuilder.DropIndex(
                name: "IX_CandidateExperiences_CurrencyId",
                table: "CandidateExperiences");

            migrationBuilder.DropColumn(
                name: "NationalityId",
                table: "Candidates");

            migrationBuilder.DropColumn(
                name: "ResidenceCityId",
                table: "Candidates");

            migrationBuilder.DropColumn(
                name: "ResidenceCountryId",
                table: "Candidates");

            migrationBuilder.DropColumn(
                name: "ResidenceDistrictId",
                table: "Candidates");

            migrationBuilder.DropColumn(
                name: "CountryId",
                table: "CandidateExperiences");

            migrationBuilder.DropColumn(
                name: "CurrencyId",
                table: "CandidateExperiences");
        }
    }
}
