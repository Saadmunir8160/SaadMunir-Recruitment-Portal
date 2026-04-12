using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infra.Migrations
{
    /// <inheritdoc />
    public partial class CleanupDealerTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop all the unused columns from Dealers table
            migrationBuilder.DropColumn(name: "AccountHolderName", table: "Dealers");
            migrationBuilder.DropColumn(name: "AccountNumber", table: "Dealers");
            migrationBuilder.DropColumn(name: "AccountType", table: "Dealers");
            migrationBuilder.DropColumn(name: "AnnualTurnover", table: "Dealers");
            migrationBuilder.DropColumn(name: "BankName", table: "Dealers");
            migrationBuilder.DropColumn(name: "BankStatementPath", table: "Dealers");
            migrationBuilder.DropColumn(name: "BusinessType", table: "Dealers");
            migrationBuilder.DropColumn(name: "CancelledChequePath", table: "Dealers");
            migrationBuilder.DropColumn(name: "CompanyName", table: "Dealers");
            migrationBuilder.DropColumn(name: "Description", table: "Dealers");
            migrationBuilder.DropColumn(name: "EmployeeCount", table: "Dealers");
            migrationBuilder.DropColumn(name: "EstablishedYear", table: "Dealers");
            migrationBuilder.DropColumn(name: "GstCertificatePath", table: "Dealers");
            migrationBuilder.DropColumn(name: "GstNumber", table: "Dealers");
            migrationBuilder.DropColumn(name: "IfsCode", table: "Dealers");
            migrationBuilder.DropColumn(name: "IncorporationCertificatePath", table: "Dealers");
            migrationBuilder.DropColumn(name: "Industry", table: "Dealers");
            migrationBuilder.DropColumn(name: "PanCardPath", table: "Dealers");
            migrationBuilder.DropColumn(name: "PanNumber", table: "Dealers");
            migrationBuilder.DropColumn(name: "PrimaryEmail", table: "Dealers");
            migrationBuilder.DropColumn(name: "PrimaryPhone", table: "Dealers");
            migrationBuilder.DropColumn(name: "RegistrationNumber", table: "Dealers");
            migrationBuilder.DropColumn(name: "SecondaryEmail", table: "Dealers");
            migrationBuilder.DropColumn(name: "SecondaryPhone", table: "Dealers");
            migrationBuilder.DropColumn(name: "Status", table: "Dealers");
            migrationBuilder.DropColumn(name: "TradeLicensePath", table: "Dealers");
            migrationBuilder.DropColumn(name: "VerificationCompletedAt", table: "Dealers");
            migrationBuilder.DropColumn(name: "VerificationReferenceNumber", table: "Dealers");
            migrationBuilder.DropColumn(name: "VerificationRequestedAt", table: "Dealers");
            migrationBuilder.DropColumn(name: "Website", table: "Dealers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
