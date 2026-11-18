using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TailorManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderHistoryEntry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActorDisplayName = table.Column<string>(type: "text", nullable: true),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderHistoryEntry", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderHistoryEntry_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderHistoryEntry_OrderId",
                table: "OrderHistoryEntry",
                column: "OrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderHistoryEntry");
        }
    }
}
