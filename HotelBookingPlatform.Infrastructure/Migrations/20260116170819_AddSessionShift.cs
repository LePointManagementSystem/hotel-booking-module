using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HotelBookingPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionShift : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CashSessionId",
                table: "CashTransactions",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CashSessions",
                columns: table => new
                {
                    CashSessionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HotelId = table.Column<int>(type: "integer", nullable: false),
                    OpenedByUserId = table.Column<string>(type: "text", nullable: false),
                    Currency = table.Column<int>(type: "integer", nullable: false),
                    Shift = table.Column<int>(type: "integer", nullable: false),
                    OpeningBalance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    OpenedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ClosingCounted = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    ClosedByUserId = table.Column<string>(type: "text", nullable: true),
                    ClosedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashSessions", x => x.CashSessionId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CashTransactions_CashSessionId",
                table: "CashTransactions",
                column: "CashSessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_CashTransactions_CashSessions_CashSessionId",
                table: "CashTransactions",
                column: "CashSessionId",
                principalTable: "CashSessions",
                principalColumn: "CashSessionId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CashTransactions_CashSessions_CashSessionId",
                table: "CashTransactions");

            migrationBuilder.DropTable(
                name: "CashSessions");

            migrationBuilder.DropIndex(
                name: "IX_CashTransactions_CashSessionId",
                table: "CashTransactions");

            migrationBuilder.DropColumn(
                name: "CashSessionId",
                table: "CashTransactions");
        }
    }
}
