using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KycApi.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Admins",
                columns: new[] { "Id", "PasswordHash", "Role", "Username" },
                values: new object[] { 1, "$2a$11$U3VcWQYVEkR92pT9XyM12ukf61eF8H/yx1Q4yx3wVFxT3vuWbDfQq", "Admin", "admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
