using System;
using AmlScreening.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmlScreening.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260330071000_AddEmiratesResidenceStatusesAndIndividualKycFks")]
    public class AddEmiratesResidenceStatusesAndIndividualKycFks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ResidenceStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResidenceStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Emirates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CountryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Emirates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Emirates_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Emirates_CountryId_Code",
                table: "Emirates",
                columns: new[] { "CountryId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResidenceStatuses_Code",
                table: "ResidenceStatuses",
                column: "Code",
                unique: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ApplicantPlaceOfBirthCountryId",
                table: "IndividualKyc",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ApplicantEmirateId",
                table: "IndividualKyc",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ApplicantResidenceStatusId",
                table: "IndividualKyc",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE IndividualKyc SET ApplicantPlaceOfBirthCountryId = TRY_CONVERT(uniqueidentifier, ApplicantCountryOfBirth)
                WHERE ApplicantCountryOfBirth IS NOT NULL AND TRY_CONVERT(uniqueidentifier, ApplicantCountryOfBirth) IS NOT NULL;

                UPDATE IndividualKyc SET ApplicantEmirateId = TRY_CONVERT(uniqueidentifier, ApplicantEmirate)
                WHERE ApplicantEmirate IS NOT NULL AND TRY_CONVERT(uniqueidentifier, ApplicantEmirate) IS NOT NULL;

                UPDATE IndividualKyc SET ApplicantResidenceStatusId = TRY_CONVERT(uniqueidentifier, ApplicantResidenceStatus)
                WHERE ApplicantResidenceStatus IS NOT NULL AND TRY_CONVERT(uniqueidentifier, ApplicantResidenceStatus) IS NOT NULL;
                """);

            migrationBuilder.DropColumn(
                name: "ApplicantCountryOfBirth",
                table: "IndividualKyc");

            migrationBuilder.DropColumn(
                name: "ApplicantEmirate",
                table: "IndividualKyc");

            migrationBuilder.DropColumn(
                name: "ApplicantResidenceStatus",
                table: "IndividualKyc");

            migrationBuilder.CreateIndex(
                name: "IX_IndividualKyc_ApplicantEmirateId",
                table: "IndividualKyc",
                column: "ApplicantEmirateId");

            migrationBuilder.CreateIndex(
                name: "IX_IndividualKyc_ApplicantPlaceOfBirthCountryId",
                table: "IndividualKyc",
                column: "ApplicantPlaceOfBirthCountryId");

            migrationBuilder.CreateIndex(
                name: "IX_IndividualKyc_ApplicantResidenceStatusId",
                table: "IndividualKyc",
                column: "ApplicantResidenceStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_IndividualKyc_Countries_ApplicantPlaceOfBirthCountryId",
                table: "IndividualKyc",
                column: "ApplicantPlaceOfBirthCountryId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_IndividualKyc_Emirates_ApplicantEmirateId",
                table: "IndividualKyc",
                column: "ApplicantEmirateId",
                principalTable: "Emirates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_IndividualKyc_ResidenceStatuses_ApplicantResidenceStatusId",
                table: "IndividualKyc",
                column: "ApplicantResidenceStatusId",
                principalTable: "ResidenceStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IndividualKyc_Countries_ApplicantPlaceOfBirthCountryId",
                table: "IndividualKyc");

            migrationBuilder.DropForeignKey(
                name: "FK_IndividualKyc_Emirates_ApplicantEmirateId",
                table: "IndividualKyc");

            migrationBuilder.DropForeignKey(
                name: "FK_IndividualKyc_ResidenceStatuses_ApplicantResidenceStatusId",
                table: "IndividualKyc");

            migrationBuilder.DropIndex(
                name: "IX_IndividualKyc_ApplicantEmirateId",
                table: "IndividualKyc");

            migrationBuilder.DropIndex(
                name: "IX_IndividualKyc_ApplicantPlaceOfBirthCountryId",
                table: "IndividualKyc");

            migrationBuilder.DropIndex(
                name: "IX_IndividualKyc_ApplicantResidenceStatusId",
                table: "IndividualKyc");

            migrationBuilder.AddColumn<string>(
                name: "ApplicantCountryOfBirth",
                table: "IndividualKyc",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApplicantEmirate",
                table: "IndividualKyc",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApplicantResidenceStatus",
                table: "IndividualKyc",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE IndividualKyc SET ApplicantCountryOfBirth = CAST(ApplicantPlaceOfBirthCountryId AS nvarchar(36))
                WHERE ApplicantPlaceOfBirthCountryId IS NOT NULL;

                UPDATE IndividualKyc SET ApplicantEmirate = CAST(ApplicantEmirateId AS nvarchar(36))
                WHERE ApplicantEmirateId IS NOT NULL;

                UPDATE IndividualKyc SET ApplicantResidenceStatus = CAST(ApplicantResidenceStatusId AS nvarchar(36))
                WHERE ApplicantResidenceStatusId IS NOT NULL;
                """);

            migrationBuilder.DropColumn(
                name: "ApplicantPlaceOfBirthCountryId",
                table: "IndividualKyc");

            migrationBuilder.DropColumn(
                name: "ApplicantEmirateId",
                table: "IndividualKyc");

            migrationBuilder.DropColumn(
                name: "ApplicantResidenceStatusId",
                table: "IndividualKyc");

            migrationBuilder.DropTable(
                name: "Emirates");

            migrationBuilder.DropTable(
                name: "ResidenceStatuses");
        }
    }
}
