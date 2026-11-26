using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TailorManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFittings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FittingAppointment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Outcome = table.Column<string>(type: "text", nullable: true),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FittingAppointment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FittingAppointment_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_FittingAppointment_OrderId",
                table: "FittingAppointment",
                column: "OrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FittingAppointment");
        }
    }
}
