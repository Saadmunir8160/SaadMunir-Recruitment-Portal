using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infra.Migrations
{
    /// <inheritdoc />
    public partial class Addfilescolumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "Vendors");

            migrationBuilder.AddColumn<string>(
                name: "CRCertificateFilePath",
                table: "Vendors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CompanyProfileFilePath",
                table: "Vendors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ISO14001_2015FilePath",
                table: "Vendors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ISO45001_2018FilePath",
                table: "Vendors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ISO9001_2015FilePath",
                table: "Vendors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VatCertificateFilePath",
                table: "Vendors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CRCertificateFilePath",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "CompanyProfileFilePath",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "ISO14001_2015FilePath",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "ISO45001_2018FilePath",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "ISO9001_2015FilePath",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "VatCertificateFilePath",
                table: "Vendors");

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "Vendors",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
