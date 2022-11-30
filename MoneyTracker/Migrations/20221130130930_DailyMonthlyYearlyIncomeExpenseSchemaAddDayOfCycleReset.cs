using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoneyTracker.Migrations
{
    /// <inheritdoc />
    public partial class DailyMonthlyYearlyIncomeExpenseSchemaAddDayOfCycleReset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "exec('\nALTER VIEW [dbo].[YearlyUserData] as\n    with expenses as\n    (\n        select \n            DATEADD(day,u.DayOfCycleReset - 1, y.y) as ''Date'',\n            COALESCE(cAST(SUM(e.Value) as real), 0) as ''YearlyExpense'',\n            u.Id\n        from AllYears y\n            CROSS JOIN AspNetUsers u\n            LEFT JOIN Expense e on e.DateOfExpense >= DATEADD(day,u.DayOfCycleReset - 1, y.y) AND e.DateOfExpense < DATEADD(year, 1, DATEADD(day,u.DayOfCycleReset - 1, y.y)) AND e.OwnerId = u.Id\n        GROUP by y.y, u.id, u.DayOfCycleReset\n    ),\n    incomes as \n    (\n        select \n            DATEADD(day,u.DayOfCycleReset - 1, y.y) as ''Date'',\n            COALESCE(cAST(SUM(i.Value) as real), 0) as ''YearlyIncome'',\n            u.Id\n        from AllYears y\n            CROSS JOIN AspNetUsers u\n            LEFT JOIN Income i on i.Date >= DATEADD(day,u.DayOfCycleReset - 1, y.y) AND i.Date < DATEADD(year, 1, DATEADD(day,u.DayOfCycleReset - 1, y.y)) AND i.OwnerId = u.Id\n        GROUP by y.y, u.id, u.DayOfCycleReset\n    )\n    SELECT e.date,e.YearlyExpense, i.YearlyIncome,e.Id FROM expenses e JOIN incomes i on e.Date = i.Date and e.Id = i.Id')\nGO\nexec('\nALTER VIEW [dbo].[MonthlyUserData] as\n    with expenses as\n    (\n        select \n            DATEADD(day,u.DayOfCycleReset - 1,m.m) as ''Date'',\n            COALESCE(cAST(SUM(e.Value) as real), 0) as ''MonthlyExpense'',\n            u.Id\n        from AllMonths m\n            CROSS JOIN AspNetUsers u\n            LEFT JOIN Expense e on e.DateOfExpense >= DATEADD(day,u.DayOfCycleReset - 1,m.m) AND e.DateOfExpense < DATEADD(month, 1, DATEADD(day,u.DayOfCycleReset - 1,m.m)) AND e.OwnerId = u.Id\n        GROUP by m.m,u.id, u.DayOfCycleReset\n    ),\n    incomes as \n    (\n        select \n            DATEADD(day,u.DayOfCycleReset - 1,m.m) as ''Date'',\n            COALESCE(cAST(SUM(i.Value) as real), 0) as ''MonthlyIncome'',\n            u.Id\n        from AllMonths m\n            CROSS JOIN AspNetUsers u\n            LEFT JOIN Income i on i.Date >= DATEADD(day,u.DayOfCycleReset - 1,m.m) AND i.Date < DATEADD(month, 1, DATEADD(day,u.DayOfCycleReset - 1,m.m)) AND i.OwnerId = u.Id\n        GROUP by m.m,u.id, u.DayOfCycleReset\n    )\n    SELECT e.date,e.MonthlyExpense, i.MonthlyIncome,e.Id FROM expenses e JOIN incomes i on e.Date = i.Date and e.Id = i.Id\n')\nGO"
                );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
