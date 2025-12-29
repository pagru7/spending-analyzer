using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpendingAnalyzer.Migrations
{
    /// <inheritdoc />
    public partial class transactionimprovement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Balance",
                table: "BankAccounts");

            migrationBuilder.RenameColumn(
                name: "CreationDate",
                table: "BankAccounts",
                newName: "CreatedAt");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ImportedTransactions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Banks",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ImportedTransactions");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Banks");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "BankAccounts",
                newName: "CreationDate");

            migrationBuilder.AddColumn<decimal>(
                name: "Balance",
                table: "BankAccounts",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
