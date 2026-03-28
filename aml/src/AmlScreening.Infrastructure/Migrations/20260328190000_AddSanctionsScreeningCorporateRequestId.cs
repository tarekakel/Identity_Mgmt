using System;
using AmlScreening.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmlScreening.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260328190000_AddSanctionsScreeningCorporateRequestId")]
public class AddSanctionsScreeningCorporateRequestId : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "CorporateScreeningRequestId",
            table: "SanctionsScreenings",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_SanctionsScreenings_CorporateScreeningRequestId",
            table: "SanctionsScreenings",
            column: "CorporateScreeningRequestId");

        migrationBuilder.AddForeignKey(
            name: "FK_SanctionsScreenings_CorporateScreeningRequests_CorporateScreeningRequestId",
            table: "SanctionsScreenings",
            column: "CorporateScreeningRequestId",
            principalTable: "CorporateScreeningRequests",
            principalColumn: "Id",
            onDelete: ReferentialAction.SetNull);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_SanctionsScreenings_CorporateScreeningRequests_CorporateScreeningRequestId",
            table: "SanctionsScreenings");

        migrationBuilder.DropIndex(
            name: "IX_SanctionsScreenings_CorporateScreeningRequestId",
            table: "SanctionsScreenings");

        migrationBuilder.DropColumn(
            name: "CorporateScreeningRequestId",
            table: "SanctionsScreenings");
    }
}
