using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RecruitmentAPI.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLookupTablesAndCandidateQualifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    CountryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Iso3Code = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Iso2Code = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    PhoneCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsCustom = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.CountryId);
                });

            migrationBuilder.CreateTable(
                name: "Currencies",
                columns: table => new
                {
                    CurrencyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsCustom = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currencies", x => x.CurrencyId);
                });

            migrationBuilder.CreateTable(
                name: "MajorFieldsOfStudy",
                columns: table => new
                {
                    MajorFieldOfStudyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsCustom = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MajorFieldsOfStudy", x => x.MajorFieldOfStudyId);
                });

            migrationBuilder.CreateTable(
                name: "QualificationTypes",
                columns: table => new
                {
                    QualificationTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QualificationTypes", x => x.QualificationTypeId);
                });

            migrationBuilder.CreateTable(
                name: "Institutions",
                columns: table => new
                {
                    InstitutionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    CountryId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsCustom = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Institutions", x => x.InstitutionId);
                    table.ForeignKey(
                        name: "FK_Institutions_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "CountryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Nationalities",
                columns: table => new
                {
                    NationalityId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CountryId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsCustom = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nationalities", x => x.NationalityId);
                    table.ForeignKey(
                        name: "FK_Nationalities_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "CountryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Regions",
                columns: table => new
                {
                    RegionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CountryId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsCustom = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Regions", x => x.RegionId);
                    table.ForeignKey(
                        name: "FK_Regions_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "CountryId");
                });

            migrationBuilder.CreateTable(
                name: "Certificates",
                columns: table => new
                {
                    CertificateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    QualificationTypeId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsCustom = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Certificates", x => x.CertificateId);
                    table.ForeignKey(
                        name: "FK_Certificates_QualificationTypes_QualificationTypeId",
                        column: x => x.QualificationTypeId,
                        principalTable: "QualificationTypes",
                        principalColumn: "QualificationTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Degrees",
                columns: table => new
                {
                    DegreeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    QualificationTypeId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsCustom = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Degrees", x => x.DegreeId);
                    table.ForeignKey(
                        name: "FK_Degrees_QualificationTypes_QualificationTypeId",
                        column: x => x.QualificationTypeId,
                        principalTable: "QualificationTypes",
                        principalColumn: "QualificationTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    CityId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CountryId = table.Column<int>(type: "int", nullable: false),
                    RegionId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsCustom = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.CityId);
                    table.ForeignKey(
                        name: "FK_Cities_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "CountryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Cities_Regions_RegionId",
                        column: x => x.RegionId,
                        principalTable: "Regions",
                        principalColumn: "RegionId");
                });

            migrationBuilder.CreateTable(
                name: "CandidateQualifications",
                columns: table => new
                {
                    CandidateQualificationId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CandidateId = table.Column<long>(type: "bigint", nullable: false),
                    CandidateEducationId = table.Column<long>(type: "bigint", nullable: true),
                    QualificationTypeId = table.Column<int>(type: "int", nullable: false),
                    DegreeId = table.Column<int>(type: "int", nullable: true),
                    CertificateId = table.Column<int>(type: "int", nullable: true),
                    MajorFieldOfStudyId = table.Column<int>(type: "int", nullable: true),
                    InstitutionId = table.Column<int>(type: "int", nullable: true),
                    CountryId = table.Column<int>(type: "int", nullable: true),
                    GraduationYear = table.Column<int>(type: "int", nullable: true),
                    GradeOrGPA = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DataSource = table.Column<byte>(type: "tinyint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidateQualifications", x => x.CandidateQualificationId);
                    table.ForeignKey(
                        name: "FK_CandidateQualifications_CandidateEducations_CandidateEducationId",
                        column: x => x.CandidateEducationId,
                        principalTable: "CandidateEducations",
                        principalColumn: "CandidateEducationId");
                    table.ForeignKey(
                        name: "FK_CandidateQualifications_Candidates_CandidateId",
                        column: x => x.CandidateId,
                        principalTable: "Candidates",
                        principalColumn: "CandidateId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CandidateQualifications_Certificates_CertificateId",
                        column: x => x.CertificateId,
                        principalTable: "Certificates",
                        principalColumn: "CertificateId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CandidateQualifications_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "CountryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CandidateQualifications_Degrees_DegreeId",
                        column: x => x.DegreeId,
                        principalTable: "Degrees",
                        principalColumn: "DegreeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CandidateQualifications_Institutions_InstitutionId",
                        column: x => x.InstitutionId,
                        principalTable: "Institutions",
                        principalColumn: "InstitutionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CandidateQualifications_MajorFieldsOfStudy_MajorFieldOfStudyId",
                        column: x => x.MajorFieldOfStudyId,
                        principalTable: "MajorFieldsOfStudy",
                        principalColumn: "MajorFieldOfStudyId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CandidateQualifications_QualificationTypes_QualificationTypeId",
                        column: x => x.QualificationTypeId,
                        principalTable: "QualificationTypes",
                        principalColumn: "QualificationTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "CountryId", "IsActive", "IsCustom", "Iso2Code", "Iso3Code", "Name", "PhoneCode" },
                values: new object[] { 1, true, true, "OT", "OTH", "Other", null });

            migrationBuilder.InsertData(
                table: "Currencies",
                columns: new[] { "CurrencyId", "Code", "IsActive", "IsCustom", "Name", "Symbol" },
                values: new object[,]
                {
                    { 1, "SAR", true, false, "Saudi Riyal", "﷼" },
                    { 2, "USD", true, false, "US Dollar", "$" },
                    { 3, "EUR", true, false, "Euro", "€" },
                    { 4, "GBP", true, false, "British Pound", "£" },
                    { 5, "AED", true, false, "UAE Dirham", "د.إ" },
                    { 6, "KWD", true, false, "Kuwaiti Dinar", "د.ك" },
                    { 7, "QAR", true, false, "Qatari Riyal", "ر.ق" },
                    { 8, "BHD", true, false, "Bahraini Dinar", "د.ب" },
                    { 9, "OMR", true, false, "Omani Rial", "ر.ع" },
                    { 10, "EGP", true, false, "Egyptian Pound", "£" },
                    { 11, "INR", true, false, "Indian Rupee", "₹" },
                    { 12, "PKR", true, false, "Pakistani Rupee", "₨" },
                    { 13, "JPY", true, false, "Japanese Yen", "¥" },
                    { 14, "CNY", true, false, "Chinese Yuan", "¥" },
                    { 15, "CAD", true, false, "Canadian Dollar", "$" },
                    { 16, "AUD", true, false, "Australian Dollar", "$" },
                    { 17, "OTH", true, true, "Other", null }
                });

            migrationBuilder.InsertData(
                table: "MajorFieldsOfStudy",
                columns: new[] { "MajorFieldOfStudyId", "IsActive", "IsCustom", "Name" },
                values: new object[] { 1, true, true, "Other" });

            migrationBuilder.InsertData(
                table: "QualificationTypes",
                columns: new[] { "QualificationTypeId", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, true, "Degree" },
                    { 2, true, "Certificate" }
                });

            migrationBuilder.InsertData(
                table: "Certificates",
                columns: new[] { "CertificateId", "IsActive", "IsCustom", "Name", "QualificationTypeId" },
                values: new object[] { 1, true, true, "Other", 2 });

            migrationBuilder.InsertData(
                table: "Degrees",
                columns: new[] { "DegreeId", "IsActive", "IsCustom", "Name", "QualificationTypeId" },
                values: new object[] { 1, true, true, "Other", 1 });

            migrationBuilder.InsertData(
                table: "Institutions",
                columns: new[] { "InstitutionId", "CountryId", "IsActive", "IsCustom", "Name" },
                values: new object[] { 1, 1, true, true, "Other" });

            migrationBuilder.InsertData(
                table: "Nationalities",
                columns: new[] { "NationalityId", "CountryId", "IsActive", "IsCustom", "Name" },
                values: new object[] { 1, 1, true, true, "Other" });

            migrationBuilder.CreateIndex(
                name: "IX_CandidateQualifications_CandidateEducationId",
                table: "CandidateQualifications",
                column: "CandidateEducationId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateQualifications_CandidateId",
                table: "CandidateQualifications",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateQualifications_CertificateId",
                table: "CandidateQualifications",
                column: "CertificateId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateQualifications_CountryId",
                table: "CandidateQualifications",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateQualifications_DegreeId",
                table: "CandidateQualifications",
                column: "DegreeId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateQualifications_InstitutionId",
                table: "CandidateQualifications",
                column: "InstitutionId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateQualifications_MajorFieldOfStudyId",
                table: "CandidateQualifications",
                column: "MajorFieldOfStudyId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateQualifications_QualificationTypeId",
                table: "CandidateQualifications",
                column: "QualificationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_Name_QualificationTypeId",
                table: "Certificates",
                columns: new[] { "Name", "QualificationTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_QualificationTypeId",
                table: "Certificates",
                column: "QualificationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Cities_CountryId",
                table: "Cities",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Cities_Name_CountryId_RegionId",
                table: "Cities",
                columns: new[] { "Name", "CountryId", "RegionId" },
                unique: true,
                filter: "[RegionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Cities_RegionId",
                table: "Cities",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_Countries_Iso2Code",
                table: "Countries",
                column: "Iso2Code",
                unique: true,
                filter: "[Iso2Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Countries_Iso3Code",
                table: "Countries",
                column: "Iso3Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Countries_Name",
                table: "Countries",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Currencies_Code",
                table: "Currencies",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Currencies_Name",
                table: "Currencies",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Degrees_Name_QualificationTypeId",
                table: "Degrees",
                columns: new[] { "Name", "QualificationTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Degrees_QualificationTypeId",
                table: "Degrees",
                column: "QualificationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Institutions_CountryId",
                table: "Institutions",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Institutions_Name_CountryId",
                table: "Institutions",
                columns: new[] { "Name", "CountryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MajorFieldsOfStudy_Name",
                table: "MajorFieldsOfStudy",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Nationalities_CountryId",
                table: "Nationalities",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Nationalities_Name",
                table: "Nationalities",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QualificationTypes_Name",
                table: "QualificationTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Regions_CountryId",
                table: "Regions",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Regions_Name_CountryId",
                table: "Regions",
                columns: new[] { "Name", "CountryId" },
                unique: true,
                filter: "[CountryId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CandidateQualifications");

            migrationBuilder.DropTable(
                name: "Cities");

            migrationBuilder.DropTable(
                name: "Currencies");

            migrationBuilder.DropTable(
                name: "Nationalities");

            migrationBuilder.DropTable(
                name: "Certificates");

            migrationBuilder.DropTable(
                name: "Degrees");

            migrationBuilder.DropTable(
                name: "Institutions");

            migrationBuilder.DropTable(
                name: "MajorFieldsOfStudy");

            migrationBuilder.DropTable(
                name: "Regions");

            migrationBuilder.DropTable(
                name: "QualificationTypes");

            migrationBuilder.DropTable(
                name: "Countries");
        }
    }
}
