using System;
using AmlScreening.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmlScreening.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260328120000_AddCorporateScreening")]
public class AddCorporateScreening : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "CorporateScreeningRequests",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CompanyCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                FullName = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                CountryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                DateOfRegistration = table.Column<DateTime>(type: "datetime2", nullable: true),
                TradeLicenceNo = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                Address = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                MatchThreshold = table.Column<int>(type: "int", nullable: false),
                CheckPepUkOnly = table.Column<bool>(type: "bit", nullable: false),
                CheckSanctions = table.Column<bool>(type: "bit", nullable: false),
                CheckProfileOfInterest = table.Column<bool>(type: "bit", nullable: false),
                CheckDisqualifiedDirectorUkOnly = table.Column<bool>(type: "bit", nullable: false),
                CheckReputationalRiskExposure = table.Column<bool>(type: "bit", nullable: false),
                CheckRegulatoryEnforcementList = table.Column<bool>(type: "bit", nullable: false),
                CheckInsolvencyUkIreland = table.Column<bool>(type: "bit", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CorporateScreeningRequests", x => x.Id);
                table.ForeignKey(
                    name: "FK_CorporateScreeningRequests_Countries_CountryId",
                    column: x => x.CountryId,
                    principalTable: "Countries",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_CorporateScreeningRequests_Customers_CustomerId",
                    column: x => x.CustomerId,
                    principalTable: "Customers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "CorporateScreeningCompanyDocuments",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CorporateScreeningRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                DocumentNo = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                IssuedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                Details = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CorporateScreeningCompanyDocuments", x => x.Id);
                table.ForeignKey(
                    name: "FK_CorporateScreeningCompanyDocuments_CorporateScreeningRequests_CorporateScreeningRequestId",
                    column: x => x.CorporateScreeningRequestId,
                    principalTable: "CorporateScreeningRequests",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "CorporateScreeningShareholders",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CorporateScreeningRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                FullName = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                NationalityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                SharePercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CorporateScreeningShareholders", x => x.Id);
                table.ForeignKey(
                    name: "FK_CorporateScreeningShareholders_CorporateScreeningRequests_CorporateScreeningRequestId",
                    column: x => x.CorporateScreeningRequestId,
                    principalTable: "CorporateScreeningRequests",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_CorporateScreeningShareholders_Nationalities_NationalityId",
                    column: x => x.NationalityId,
                    principalTable: "Nationalities",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "CorporateScreeningShareholderDocuments",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CorporateScreeningShareholderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                DocumentNo = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                IssuedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                Details = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CorporateScreeningShareholderDocuments", x => x.Id);
                table.ForeignKey(
                    name: "FK_CorporateScreeningShareholderDocuments_CorporateScreeningShareholders_CorporateScreeningShareholderId",
                    column: x => x.CorporateScreeningShareholderId,
                    principalTable: "CorporateScreeningShareholders",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(name: "IX_CorporateScreeningRequests_CustomerId", table: "CorporateScreeningRequests", column: "CustomerId");
        migrationBuilder.CreateIndex(name: "IX_CorporateScreeningRequests_TenantId", table: "CorporateScreeningRequests", column: "TenantId");
        migrationBuilder.CreateIndex(name: "IX_CorporateScreeningCompanyDocuments_CorporateScreeningRequestId", table: "CorporateScreeningCompanyDocuments", column: "CorporateScreeningRequestId");
        migrationBuilder.CreateIndex(name: "IX_CorporateScreeningShareholders_CorporateScreeningRequestId", table: "CorporateScreeningShareholders", column: "CorporateScreeningRequestId");
        migrationBuilder.CreateIndex(name: "IX_CorporateScreeningShareholders_NationalityId", table: "CorporateScreeningShareholders", column: "NationalityId");
        migrationBuilder.CreateIndex(name: "IX_CorporateScreeningShareholderDocuments_CorporateScreeningShareholderId", table: "CorporateScreeningShareholderDocuments", column: "CorporateScreeningShareholderId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "CorporateScreeningShareholderDocuments");
        migrationBuilder.DropTable(name: "CorporateScreeningCompanyDocuments");
        migrationBuilder.DropTable(name: "CorporateScreeningShareholders");
        migrationBuilder.DropTable(name: "CorporateScreeningRequests");
    }
}
