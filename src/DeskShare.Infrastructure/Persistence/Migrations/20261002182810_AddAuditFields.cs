using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeskShare.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreationTime",
                table: "Employees",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "Employees",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModificationTime",
                table: "Employees",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifierId",
                table: "Employees",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationTime",
                table: "Desks",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "Desks",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModificationTime",
                table: "Desks",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifierId",
                table: "Desks",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationTime",
                table: "Bookings",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "Bookings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModificationTime",
                table: "Bookings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifierId",
                table: "Bookings",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreationTime",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "LastModificationTime",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "LastModifierId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "CreationTime",
                table: "Desks");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "Desks");

            migrationBuilder.DropColumn(
                name: "LastModificationTime",
                table: "Desks");

            migrationBuilder.DropColumn(
                name: "LastModifierId",
                table: "Desks");

            migrationBuilder.DropColumn(
                name: "CreationTime",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "LastModificationTime",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "LastModifierId",
                table: "Bookings");
        }
    }
}
