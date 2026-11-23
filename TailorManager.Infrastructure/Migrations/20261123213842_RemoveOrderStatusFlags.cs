using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TailorManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveOrderStatusFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDelivered",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IsReadyForPickup",
                table: "Orders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDelivered",
                table: "Orders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsReadyForPickup",
                table: "Orders",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
