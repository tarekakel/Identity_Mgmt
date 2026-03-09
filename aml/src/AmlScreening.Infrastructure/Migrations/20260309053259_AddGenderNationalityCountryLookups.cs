using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmlScreening.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGenderNationalityCountryLookups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var maleId = new Guid("33333333-0000-0000-0000-000000000001");
            var femaleId = new Guid("33333333-0000-0000-0000-000000000002");
            var otherId = new Guid("33333333-0000-0000-0000-000000000003");
            var uaeId = new Guid("44444444-0000-0000-0000-000000000001");
            var saudiId = new Guid("44444444-0000-0000-0000-000000000002");
            var egyptId = new Guid("44444444-0000-0000-0000-000000000003");
            var indiaId = new Guid("44444444-0000-0000-0000-000000000004");
            var pakistanId = new Guid("44444444-0000-0000-0000-000000000005");
            var ukId = new Guid("44444444-0000-0000-0000-000000000006");
            var usaId = new Guid("44444444-0000-0000-0000-000000000007");
            var otherCountryId = new Guid("44444444-0000-0000-0000-000000000008");

            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Genders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Nationalities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nationalities", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Genders",
                columns: new[] { "Id", "Code", "Name" },
                values: new object[,]
                {
                    { maleId, "Male", "Male" },
                    { femaleId, "Female", "Female" },
                    { otherId, "Other", "Other" }
                });

            migrationBuilder.InsertData(
                table: "Nationalities",
                columns: new[] { "Id", "Code", "Name" },
                values: new object[,]
                {
                    { uaeId, "AE", "UAE" },
                    { saudiId, "SA", "Saudi Arabia" },
                    { egyptId, "EG", "Egypt" },
                    { indiaId, "IN", "India" },
                    { pakistanId, "PK", "Pakistan" },
                    { ukId, "GB", "United Kingdom" },
                    { usaId, "US", "United States" },
                    { otherCountryId, "Other", "Other" }
                });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Id", "Code", "Name" },
                values: new object[,]
                {
                    { uaeId, "AE", "UAE" },
                    { saudiId, "SA", "Saudi Arabia" },
                    { egyptId, "EG", "Egypt" },
                    { indiaId, "IN", "India" },
                    { pakistanId, "PK", "Pakistan" },
                    { ukId, "GB", "United Kingdom" },
                    { usaId, "US", "United States" },
                    { otherCountryId, "Other", "Other" }
                });

            migrationBuilder.AddColumn<Guid>(
                name: "GenderId",
                table: "Customers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "NationalityId",
                table: "Customers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CountryOfResidenceId",
                table: "Customers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE c SET c.GenderId = g.Id
                FROM Customers c
                INNER JOIN Genders g ON (LTRIM(RTRIM(LOWER(c.Gender))) = LOWER(g.Code) OR LTRIM(RTRIM(LOWER(c.Gender))) = LOWER(g.Name))
                WHERE c.Gender IS NOT NULL AND LTRIM(RTRIM(c.Gender)) <> '';
            ");
            migrationBuilder.Sql(@"
                UPDATE c SET c.NationalityId = n.Id
                FROM Customers c
                INNER JOIN Nationalities n ON (LTRIM(RTRIM(LOWER(c.Nationality))) = LOWER(n.Code) OR LTRIM(RTRIM(LOWER(c.Nationality))) = LOWER(n.Name))
                WHERE c.Nationality IS NOT NULL AND LTRIM(RTRIM(c.Nationality)) <> '';
            ");
            migrationBuilder.Sql(@"
                UPDATE c SET c.CountryOfResidenceId = co.Id
                FROM Customers c
                INNER JOIN Countries co ON (LTRIM(RTRIM(LOWER(c.CountryOfResidence))) = LOWER(co.Code) OR LTRIM(RTRIM(LOWER(c.CountryOfResidence))) = LOWER(co.Name))
                WHERE c.CountryOfResidence IS NOT NULL AND LTRIM(RTRIM(c.CountryOfResidence)) <> '';
            ");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Nationality",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CountryOfResidence",
                table: "Customers");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_CountryOfResidenceId",
                table: "Customers",
                column: "CountryOfResidenceId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_GenderId",
                table: "Customers",
                column: "GenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_NationalityId",
                table: "Customers",
                column: "NationalityId");

            migrationBuilder.CreateIndex(
                name: "IX_Countries_Code",
                table: "Countries",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Genders_Code",
                table: "Genders",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Nationalities_Code",
                table: "Nationalities",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Countries_CountryOfResidenceId",
                table: "Customers",
                column: "CountryOfResidenceId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Genders_GenderId",
                table: "Customers",
                column: "GenderId",
                principalTable: "Genders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Nationalities_NationalityId",
                table: "Customers",
                column: "NationalityId",
                principalTable: "Nationalities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Countries_CountryOfResidenceId",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Genders_GenderId",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Nationalities_NationalityId",
                table: "Customers");

            migrationBuilder.DropTable(
                name: "Countries");

            migrationBuilder.DropTable(
                name: "Genders");

            migrationBuilder.DropTable(
                name: "Nationalities");

            migrationBuilder.DropIndex(
                name: "IX_Customers_CountryOfResidenceId",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_GenderId",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_NationalityId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CountryOfResidenceId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "GenderId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "NationalityId",
                table: "Customers");

            migrationBuilder.AddColumn<string>(
                name: "CountryOfResidence",
                table: "Customers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "Customers",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nationality",
                table: "Customers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
