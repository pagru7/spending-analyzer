using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpendingAnalyzer.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalIdParsedUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ImportedTransactions_AccountId",
                table: "ImportedTransactions");

            migrationBuilder.AddColumn<int>(
                name: "ExternalIdParsed",
                table: "ImportedTransactions",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImportedTransactions_AccountId_ExternalIdParsed",
                table: "ImportedTransactions",
                columns: new[] { "AccountId", "ExternalIdParsed" },
                unique: true,
                filter: "\"ExternalIdParsed\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ImportedTransactions_AccountId_ExternalIdParsed",
                table: "ImportedTransactions");

            migrationBuilder.DropColumn(
                name: "ExternalIdParsed",
                table: "ImportedTransactions");

            migrationBuilder.CreateIndex(
                name: "IX_ImportedTransactions_AccountId",
                table: "ImportedTransactions",
                column: "AccountId");
        }
    }
}
