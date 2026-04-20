using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecruitmentAPI.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddArabicNameColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "Regions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "QualificationTypes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "Nationalities",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "MajorFieldsOfStudy",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "Institutions",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "Degrees",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "Currencies",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "Countries",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "Cities",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "Certificates",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Certificates",
                keyColumn: "CertificateId",
                keyValue: 1,
                column: "NameAr",
                value: null);

            migrationBuilder.UpdateData(
                table: "Countries",
                keyColumn: "CountryId",
                keyValue: 1,
                column: "NameAr",
                value: null);

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "CurrencyId",
                keyValue: 1,
                column: "NameAr",
                value: null);

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "CurrencyId",
                keyValue: 2,
                column: "NameAr",
                value: null);

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "CurrencyId",
                keyValue: 3,
                column: "NameAr",
                value: null);

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "CurrencyId",
                keyValue: 4,
                column: "NameAr",
                value: null);

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "CurrencyId",
                keyValue: 5,
                column: "NameAr",
                value: null);

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "CurrencyId",
                keyValue: 6,
                column: "NameAr",
                value: null);

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "CurrencyId",
                keyValue: 7,
                column: "NameAr",
                value: null);

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "CurrencyId",
                keyValue: 8,
                column: "NameAr",
                value: null);

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "CurrencyId",
                keyValue: 9,
                column: "NameAr",
                value: null);

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "CurrencyId",
                keyValue: 10,
                column: "NameAr",
                value: null);

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "CurrencyId",
                keyValue: 11,
                column: "NameAr",
                value: null);

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "CurrencyId",
                keyValue: 12,
                column: "NameAr",
                value: null);

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "CurrencyId",
                keyValue: 13,
                column: "NameAr",
                value: null);

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "CurrencyId",
                keyValue: 14,
                column: "NameAr",
                value: null);

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "CurrencyId",
                keyValue: 15,
                column: "NameAr",
                value: null);

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "CurrencyId",
                keyValue: 16,
                column: "NameAr",
                value: null);

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "CurrencyId",
                keyValue: 17,
                column: "NameAr",
                value: null);

            migrationBuilder.UpdateData(
                table: "Degrees",
                keyColumn: "DegreeId",
                keyValue: 1,
                column: "NameAr",
                value: null);

            migrationBuilder.UpdateData(
                table: "Institutions",
                keyColumn: "InstitutionId",
                keyValue: 1,
                column: "NameAr",
                value: null);

            migrationBuilder.UpdateData(
                table: "MajorFieldsOfStudy",
                keyColumn: "MajorFieldOfStudyId",
                keyValue: 1,
                column: "NameAr",
                value: null);

            migrationBuilder.UpdateData(
                table: "Nationalities",
                keyColumn: "NationalityId",
                keyValue: 1,
                column: "NameAr",
                value: null);

            migrationBuilder.UpdateData(
                table: "QualificationTypes",
                keyColumn: "QualificationTypeId",
                keyValue: 1,
                column: "NameAr",
                value: null);

            migrationBuilder.UpdateData(
                table: "QualificationTypes",
                keyColumn: "QualificationTypeId",
                keyValue: 2,
                column: "NameAr",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "Regions");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "QualificationTypes");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "Nationalities");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "MajorFieldsOfStudy");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "Institutions");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "Degrees");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "Currencies");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "Countries");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "Cities");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "Certificates");
        }
    }
}
