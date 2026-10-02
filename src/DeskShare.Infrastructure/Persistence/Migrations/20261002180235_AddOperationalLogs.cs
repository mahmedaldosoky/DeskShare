using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeskShare.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOperationalLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OperationalLogs",
                columns: table => new
                {
                    Fingerprint = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    ExceptionType = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Source = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Message = table.Column<string>(type: "TEXT", nullable: false),
                    StackTrace = table.Column<string>(type: "TEXT", nullable: true),
                    RequestPath = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false),
                    OccurrenceCount = table.Column<int>(type: "INTEGER", nullable: false),
                    FirstOccurredAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastOccurredAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationalLogs", x => x.Fingerprint);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OperationalLogs");
        }
    }
}
