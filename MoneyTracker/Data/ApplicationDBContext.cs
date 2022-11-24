using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;

namespace MoneyTracker.Data
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) :base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DailyUserData>(
                    eb =>
                    {
                        eb.HasNoKey();
                        eb.ToView("DailyUserData");
                    });
            modelBuilder.Entity<MonthlyUserData>(
                    eb =>
                    {
                        eb.HasNoKey();
                        eb.ToView("MonthlyUserData");
                    });
            modelBuilder.Entity<YearlyUserData>(
                    eb =>
                    {
                        eb.HasNoKey();
                        eb.ToView("YearlyUserData");
                    });
        }

        public DbSet<Category> Categories { get; set; }

        public DbSet<SubCategory> SubCategory { get; set; }

        public DbSet<Expense> Expense { get; set; }

        public DbSet<Income> Income { get; set; }

        public DbSet<DailyUserData> DailyUserData { get; set; }

        public DbSet<MonthlyUserData> MonthlyUserData { get; set; }

        public DbSet<YearlyUserData> YearlyUserData { get; set; }

    }
}
