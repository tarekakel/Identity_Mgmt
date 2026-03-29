using System;
using AmlScreening.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmlScreening.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260329170000_AddDashboardCustomerStatuses")]
    public partial class AddDashboardCustomerStatuses : Migration
    {
        private static readonly Guid AutoApprovedId = new("11111111-0000-0000-0000-000000000007");
        private static readonly Guid PendingMakerId = new("11111111-0000-0000-0000-000000000008");
        private static readonly Guid PendingCheckerId = new("11111111-0000-0000-0000-000000000009");
        private static readonly Guid PendingSchedulerId = new("11111111-0000-0000-0000-000000000010");

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Raw SQL: hand-authored migration has no Designer.cs; InsertData/DeleteData require a target model.
            migrationBuilder.Sql($@"
IF NOT EXISTS (SELECT 1 FROM CustomerStatuses WHERE Id = '{AutoApprovedId}')
    INSERT INTO CustomerStatuses (Id, Code, Name) VALUES ('{AutoApprovedId}', N'AutoApproved', N'Auto Approved');
IF NOT EXISTS (SELECT 1 FROM CustomerStatuses WHERE Id = '{PendingMakerId}')
    INSERT INTO CustomerStatuses (Id, Code, Name) VALUES ('{PendingMakerId}', N'PendingMaker', N'Pending Maker');
IF NOT EXISTS (SELECT 1 FROM CustomerStatuses WHERE Id = '{PendingCheckerId}')
    INSERT INTO CustomerStatuses (Id, Code, Name) VALUES ('{PendingCheckerId}', N'PendingChecker', N'Pending Checker');
IF NOT EXISTS (SELECT 1 FROM CustomerStatuses WHERE Id = '{PendingSchedulerId}')
    INSERT INTO CustomerStatuses (Id, Code, Name) VALUES ('{PendingSchedulerId}', N'PendingScheduler', N'Pending Scheduler');
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
DELETE FROM CustomerStatuses WHERE Id IN (
    '{AutoApprovedId}', '{PendingMakerId}', '{PendingCheckerId}', '{PendingSchedulerId}');
");
        }
    }
}
