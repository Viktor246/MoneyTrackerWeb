namespace MoneyTracker.Models
{    public class UserDashboard
    {
        public List<DailyUserData>? DailyUserData { get; set; }
        public List<MonthlyUserData>? MonthlyUserData { get; set; }
        public List<YearlyUserData>? YearlyUserData { get; set; }
        public List<Expense>? RecentExpenses { get; set; }

    }
}
