using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KycApi.Migrations
{
    /// <inheritdoc />
    public partial class AddKycReviewFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "KycApplications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                table: "KycApplications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "KycApplications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "KycApplications");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "KycApplications");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "KycApplications");
        }
    }
}
