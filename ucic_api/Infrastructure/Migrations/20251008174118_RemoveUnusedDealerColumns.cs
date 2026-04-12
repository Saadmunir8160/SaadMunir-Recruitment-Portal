using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infra.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnusedDealerColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountHolderName",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "AccountNumber",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "AccountType",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "AnnualTurnover",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "BankName",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "BankStatementPath",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "BusinessType",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "CancelledChequePath",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "EmployeeCount",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "EstablishedYear",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "GstCertificatePath",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "GstNumber",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "IfscCode",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "IncorporationCertificatePath",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "Industry",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "PanCardPath",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "PanNumber",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "PrimaryEmail",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "PrimaryPhone",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "RegistrationNumber",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "SecondaryEmail",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "SecondaryPhone",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "TradeLicensePath",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "VerificationCompletedAt",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "VerificationReferenceNumber",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "VerificationRequestedAt",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "Website",
                table: "Dealers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountHolderName",
                table: "Dealers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountNumber",
                table: "Dealers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountType",
                table: "Dealers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AnnualTurnover",
                table: "Dealers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "Dealers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankStatementPath",
                table: "Dealers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessType",
                table: "Dealers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancelledChequePath",
                table: "Dealers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "Dealers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Dealers",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmployeeCount",
                table: "Dealers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EstablishedYear",
                table: "Dealers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GstCertificatePath",
                table: "Dealers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GstNumber",
                table: "Dealers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IfscCode",
                table: "Dealers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IncorporationCertificatePath",
                table: "Dealers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Industry",
                table: "Dealers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PanCardPath",
                table: "Dealers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PanNumber",
                table: "Dealers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryEmail",
                table: "Dealers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryPhone",
                table: "Dealers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegistrationNumber",
                table: "Dealers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecondaryEmail",
                table: "Dealers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecondaryPhone",
                table: "Dealers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Dealers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TradeLicensePath",
                table: "Dealers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VerificationCompletedAt",
                table: "Dealers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VerificationReferenceNumber",
                table: "Dealers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VerificationRequestedAt",
                table: "Dealers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Website",
                table: "Dealers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
