using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeskShare.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SoftDeleteDesks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Desks_DeskId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Desks_Code",
                table: "Desks");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Desks",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Desks_Code",
                table: "Desks",
                column: "Code",
                unique: true,
                filter: "\"IsDeleted\" = 0");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Desks_DeskId",
                table: "Bookings",
                column: "DeskId",
                principalTable: "Desks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Desks_DeskId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Desks_Code",
                table: "Desks");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Desks");

            migrationBuilder.CreateIndex(
                name: "IX_Desks_Code",
                table: "Desks",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Desks_DeskId",
                table: "Bookings",
                column: "DeskId",
                principalTable: "Desks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
