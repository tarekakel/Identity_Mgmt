using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmlScreening.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOccupationAndSourceOfFundsLookups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Occupations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Occupations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SourceOfFunds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SourceOfFunds", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Occupations_Code",
                table: "Occupations",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SourceOfFunds_Code",
                table: "SourceOfFunds",
                column: "Code",
                unique: true);

            var occ1 = new Guid("55555555-0000-0000-0000-000000000001");
            var occ2 = new Guid("55555555-0000-0000-0000-000000000002");
            var occ3 = new Guid("55555555-0000-0000-0000-000000000003");
            var occ4 = new Guid("55555555-0000-0000-0000-000000000004");
            var occ5 = new Guid("55555555-0000-0000-0000-000000000005");
            var occ6 = new Guid("55555555-0000-0000-0000-000000000006");
            var sof1 = new Guid("66666666-0000-0000-0000-000000000001");
            var sof2 = new Guid("66666666-0000-0000-0000-000000000002");
            var sof3 = new Guid("66666666-0000-0000-0000-000000000003");
            var sof4 = new Guid("66666666-0000-0000-0000-000000000004");
            var sof5 = new Guid("66666666-0000-0000-0000-000000000005");

            migrationBuilder.InsertData(
                table: "Occupations",
                columns: new[] { "Id", "Code", "Name" },
                values: new object[,]
                {
                    { occ1, "Employed", "Employed" },
                    { occ2, "SelfEmployed", "Self-Employed" },
                    { occ3, "Retired", "Retired" },
                    { occ4, "Student", "Student" },
                    { occ5, "Unemployed", "Unemployed" },
                    { occ6, "Other", "Other" }
                });

            migrationBuilder.InsertData(
                table: "SourceOfFunds",
                columns: new[] { "Id", "Code", "Name" },
                values: new object[,]
                {
                    { sof1, "Salary", "Salary" },
                    { sof2, "Business", "Business" },
                    { sof3, "Investments", "Investments" },
                    { sof4, "Inheritance", "Inheritance" },
                    { sof5, "Other", "Other" }
                });

            migrationBuilder.AddColumn<Guid>(
                name: "OccupationId",
                table: "Customers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceOfFundsId",
                table: "Customers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_OccupationId",
                table: "Customers",
                column: "OccupationId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_SourceOfFundsId",
                table: "Customers",
                column: "SourceOfFundsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Occupations_OccupationId",
                table: "Customers",
                column: "OccupationId",
                principalTable: "Occupations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_SourceOfFunds_SourceOfFundsId",
                table: "Customers",
                column: "SourceOfFundsId",
                principalTable: "SourceOfFunds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropColumn(
                name: "Occupation",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "SourceOfFunds",
                table: "Customers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Occupations_OccupationId",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_SourceOfFunds_SourceOfFundsId",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_OccupationId",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_SourceOfFundsId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "OccupationId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "SourceOfFundsId",
                table: "Customers");

            migrationBuilder.AddColumn<string>(
                name: "Occupation",
                table: "Customers",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceOfFunds",
                table: "Customers",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.DropTable(
                name: "Occupations");

            migrationBuilder.DropTable(
                name: "SourceOfFunds");
        }
    }
}
