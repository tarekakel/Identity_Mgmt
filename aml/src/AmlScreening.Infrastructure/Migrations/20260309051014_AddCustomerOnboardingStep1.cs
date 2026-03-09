using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmlScreening.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerOnboardingStep1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var draftId = new Guid("11111111-0000-0000-0000-000000000001");
            var pendingScreeningId = new Guid("11111111-0000-0000-0000-000000000002");
            var pendingApprovalId = new Guid("11111111-0000-0000-0000-000000000003");
            var approvedId = new Guid("11111111-0000-0000-0000-000000000004");
            var rejectedId = new Guid("11111111-0000-0000-0000-000000000005");
            var escalatedId = new Guid("11111111-0000-0000-0000-000000000006");
            var individualId = new Guid("22222222-0000-0000-0000-000000000001");
            var corporateId = new Guid("22222222-0000-0000-0000-000000000002");

            migrationBuilder.CreateTable(
                name: "CustomerStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CustomerTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerTypes", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "CustomerStatuses",
                columns: new[] { "Id", "Code", "Name" },
                values: new object[,]
                {
                    { draftId, "Draft", "Draft" },
                    { pendingScreeningId, "PendingScreening", "Pending Screening" },
                    { pendingApprovalId, "PendingApproval", "Pending Approval" },
                    { approvedId, "Approved", "Approved" },
                    { rejectedId, "Rejected", "Rejected" },
                    { escalatedId, "Escalated", "Escalated" }
                });

            migrationBuilder.InsertData(
                table: "CustomerTypes",
                columns: new[] { "Id", "Code", "Name" },
                values: new object[,]
                {
                    { individualId, "Individual", "Individual" },
                    { corporateId, "Corporate", "Corporate" }
                });

            migrationBuilder.AlterColumn<string>(
                name: "RiskClassification",
                table: "Customers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<string>(
                name: "AccountPurpose",
                table: "Customers",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AnnualIncome",
                table: "Customers",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Customers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Customers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CountryOfResidence",
                table: "Customers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerNumber",
                table: "Customers",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerTypeId",
                table: "Customers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: individualId);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Customers",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmployerName",
                table: "Customers",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExpectedMonthlyTransactionValue",
                table: "Customers",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExpectedMonthlyTransactionVolume",
                table: "Customers",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Customers",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "Customers",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Customers",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NationalId",
                table: "Customers",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PassportExpiryDate",
                table: "Customers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PassportNumber",
                table: "Customers",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Customers",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RiskLevel",
                table: "Customers",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RiskScore",
                table: "Customers",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StatusId",
                table: "Customers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: pendingScreeningId);

            migrationBuilder.Sql(@"
                UPDATE c SET c.CustomerNumber = 'CUST-' + RIGHT('000000' + CAST(rn.n AS VARCHAR(6)), 6)
                FROM Customers c
                INNER JOIN (SELECT Id, ROW_NUMBER() OVER (ORDER BY CreatedAt) AS n FROM Customers WHERE CustomerNumber IS NULL) rn ON c.Id = rn.Id;
            ");
            migrationBuilder.AlterColumn<string>(
                name: "CustomerNumber",
                table: "Customers",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "CUST-000000");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_CustomerNumber",
                table: "Customers",
                column: "CustomerNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_CustomerTypeId",
                table: "Customers",
                column: "CustomerTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_StatusId",
                table: "Customers",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerStatuses_Code",
                table: "CustomerStatuses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerTypes_Code",
                table: "CustomerTypes",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_CustomerStatuses_StatusId",
                table: "Customers",
                column: "StatusId",
                principalTable: "CustomerStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_CustomerTypes_CustomerTypeId",
                table: "Customers",
                column: "CustomerTypeId",
                principalTable: "CustomerTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_CustomerStatuses_StatusId",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_CustomerTypes_CustomerTypeId",
                table: "Customers");

            migrationBuilder.DropTable(
                name: "CustomerStatuses");

            migrationBuilder.DropTable(
                name: "CustomerTypes");

            migrationBuilder.DropIndex(
                name: "IX_Customers_CustomerNumber",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_CustomerTypeId",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_StatusId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "AccountPurpose",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "AnnualIncome",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CountryOfResidence",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CustomerNumber",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CustomerTypeId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "EmployerName",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "ExpectedMonthlyTransactionValue",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "ExpectedMonthlyTransactionVolume",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "NationalId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "PassportExpiryDate",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "PassportNumber",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "RiskLevel",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "RiskScore",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "StatusId",
                table: "Customers");

            migrationBuilder.AlterColumn<string>(
                name: "RiskClassification",
                table: "Customers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);
        }
    }
}
