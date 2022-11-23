using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;

namespace MoneyTracker.Data
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) :base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }

        public DbSet<MoneyTracker.Models.SubCategory> SubCategory { get; set; }

        public DbSet<MoneyTracker.Models.Expense> Expense { get; set; }

        public DbSet<MoneyTracker.Models.Income> Income { get; set; }

    }
}
