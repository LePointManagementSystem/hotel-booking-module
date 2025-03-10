using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelBookingPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixedInitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Hotels_owners_OwnerID",
                table: "Hotels");

            migrationBuilder.DropPrimaryKey(
                name: "PK_owners",
                table: "owners");

            migrationBuilder.RenameTable(
                name: "owners",
                newName: "Owners");

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "Owners",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "Owners",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Owners",
                table: "Owners",
                column: "OwnerID");

            migrationBuilder.AddForeignKey(
                name: "FK_Hotels_Owners_OwnerID",
                table: "Hotels",
                column: "OwnerID",
                principalTable: "Owners",
                principalColumn: "OwnerID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Hotels_Owners_OwnerID",
                table: "Hotels");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Owners",
                table: "Owners");

            migrationBuilder.RenameTable(
                name: "Owners",
                newName: "owners");

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "owners",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "owners",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddPrimaryKey(
                name: "PK_owners",
                table: "owners",
                column: "OwnerID");

            migrationBuilder.AddForeignKey(
                name: "FK_Hotels_owners_OwnerID",
                table: "Hotels",
                column: "OwnerID",
                principalTable: "owners",
                principalColumn: "OwnerID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
