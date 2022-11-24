using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Areas.Identity.Data;
using MoneyTracker.Data;
using System.Security.Claims;

namespace MoneyTracker.Controllers
{
    public class UserDashboardController : Controller
    {
        private readonly ApplicationDBContext _context;
        private readonly ApplicationUserContext _userContext;

        public async Task<IActionResult> Index()
        {
            string userId = this.GetUserId();
            if (userId == null) { return Redirect("/"); }
            var user = await _userContext.Users.FindAsync(this.GetUserId());
            if (user == null ) { return Redirect("/"); }
            var expenses = _context.Expense
                .Include(e => e.SubCategory)
                .Include(e => e.SubCategory.Category)
                .Where(e => e.OwnerId == this.GetUserId())
                .OrderByDescending(e => e.DateOfExpense)
                .Take(5);

            var dailyUserData = _context.DailyUserData.Where(e => e.Id == userId ).OrderBy(e => e.Date);

            var applicationDBContext = _context.Expense.Include(e => e.SubCategory).Where(e => e.OwnerId == this.GetUserId());


            return View();
        }
        private string GetUserId()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return userId;
        }
    }
}
