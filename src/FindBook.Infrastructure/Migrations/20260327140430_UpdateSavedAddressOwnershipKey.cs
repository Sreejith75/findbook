using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FindBook.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSavedAddressOwnershipKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SavedAddresses",
                table: "SavedAddresses");

            migrationBuilder.DropIndex(
                name: "IX_SavedAddresses_UserAccountId",
                table: "SavedAddresses");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SavedAddresses",
                table: "SavedAddresses",
                columns: new[] { "UserAccountId", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SavedAddresses",
                table: "SavedAddresses");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SavedAddresses",
                table: "SavedAddresses",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_SavedAddresses_UserAccountId",
                table: "SavedAddresses",
                column: "UserAccountId");
        }
    }
}
