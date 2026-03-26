using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmlScreening.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIndividualScreeningRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IndividualScreeningRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferenceId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NationalityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PlaceOfBirthCountryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IdType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    IdNumber = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    GenderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MatchThreshold = table.Column<int>(type: "int", nullable: false),
                    BirthYearRange = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_IndividualScreeningRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndividualScreeningRequests_Countries_PlaceOfBirthCountryId",
                        column: x => x.PlaceOfBirthCountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IndividualScreeningRequests_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IndividualScreeningRequests_Genders_GenderId",
                        column: x => x.GenderId,
                        principalTable: "Genders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IndividualScreeningRequests_Nationalities_NationalityId",
                        column: x => x.NationalityId,
                        principalTable: "Nationalities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IndividualScreeningRequests_CustomerId",
                table: "IndividualScreeningRequests",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_IndividualScreeningRequests_GenderId",
                table: "IndividualScreeningRequests",
                column: "GenderId");

            migrationBuilder.CreateIndex(
                name: "IX_IndividualScreeningRequests_NationalityId",
                table: "IndividualScreeningRequests",
                column: "NationalityId");

            migrationBuilder.CreateIndex(
                name: "IX_IndividualScreeningRequests_PlaceOfBirthCountryId",
                table: "IndividualScreeningRequests",
                column: "PlaceOfBirthCountryId");

            migrationBuilder.CreateIndex(
                name: "IX_IndividualScreeningRequests_TenantId",
                table: "IndividualScreeningRequests",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_IndividualScreeningRequests_TenantId_CustomerId",
                table: "IndividualScreeningRequests",
                columns: new[] { "TenantId", "CustomerId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IndividualScreeningRequests");
        }
    }
}
