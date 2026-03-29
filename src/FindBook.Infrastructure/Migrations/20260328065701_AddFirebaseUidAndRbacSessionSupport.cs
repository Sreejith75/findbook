using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FindBook.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFirebaseUidAndRbacSessionSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FirebaseUid",
                table: "UserAccounts",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAccounts_FirebaseUid",
                table: "UserAccounts",
                column: "FirebaseUid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserAccounts_FirebaseUid",
                table: "UserAccounts");

            migrationBuilder.DropColumn(
                name: "FirebaseUid",
                table: "UserAccounts");
        }
    }
}
