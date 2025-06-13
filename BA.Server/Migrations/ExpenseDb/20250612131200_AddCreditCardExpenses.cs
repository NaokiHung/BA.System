using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BA.Server.Migrations.ExpenseDb
{
    /// <inheritdoc />
    public partial class AddCreditCardExpenses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CreditCardExpenses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    Month = table.Column<int>(type: "INTEGER", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    CardName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Installments = table.Column<int>(type: "INTEGER", nullable: false),
                    IsOnlineTransaction = table.Column<bool>(type: "INTEGER", nullable: false),
                    MerchantName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    AuthorizationCode = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    CardLastFourDigits = table.Column<string>(type: "TEXT", maxLength: 4, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsBilled = table.Column<bool>(type: "INTEGER", nullable: false),
                    BilledDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsRecurring = table.Column<bool>(type: "INTEGER", nullable: false),
                    OriginalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    OriginalCurrency = table.Column<string>(type: "TEXT", maxLength: 3, nullable: true),
                    ExchangeRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditCardExpenses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CreditCardExpenses_UserId_Year_Month",
                table: "CreditCardExpenses",
                columns: new[] { "UserId", "Year", "Month" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CreditCardExpenses");
        }
    }
}