using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Areas.Identity.Data;
using MoneyTracker.Data;
using MoneyTracker.Models;
using MoneyTracker.Utility;
using NuGet.Protocol;
using System;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace MoneyTracker.Controllers
{
    public class UserDashboardController : Controller
    {
        private readonly ApplicationDBContext _context;
        private readonly ApplicationUserContext _userContext;

        public UserDashboardController(ApplicationDBContext context, ApplicationUserContext userContext)
        {
            _context = context;
            _userContext = userContext;
        }
        public async Task<IActionResult> Index()
        {
            string userId = this.GetUserId();
            if (userId == null) { return Redirect("/"); }
            var user = await _userContext.Users.FindAsync(this.GetUserId());
            if (user == null ) { return Redirect("/"); }

            var expenses = await _context.Expense
                .Include(e => e.SubCategory)
                .Include(e => e.SubCategory.Category)
                .Where(e => e.OwnerId == this.GetUserId())
                .OrderByDescending(e => e.DateOfExpense)
                .Take(5).ToListAsync();

            int year = DateTime.Now.Year, month = DateTime.Now.Month, day = DateTime.Now.Day, dayOfCycleReset = user.DayOfCycleReset;

            if (day < dayOfCycleReset)
            {
                if (month == 1)
                {
                    month = 12;
                    year--;
                }
                else
                {
                    month--;
                }
            }
            DateTime minDateDaily = new(year, month, dayOfCycleReset);

            if (month == 12)
            {
                month = 1;
                year++;
            }
            else
            {
                month++;
            }
            DateTime maxDateDaily = new(year, month, dayOfCycleReset);

            DateTime startOfYearCiclye = new(DateTime.Now.Year,1, 1);
            DateTime endOfYearCiclye = new(DateTime.Now.Year + 1,1, 1);

            var dailyUserData = await _context.DailyUserData
                .Where(e => e.Id == userId && e.Date >= minDateDaily && e.Date < maxDateDaily)
                .OrderBy(e => e.Date).ToListAsync();
            var monthlyUserData = await _context.MonthlyUserData
                .Where(e => e.Id == userId && e.Date >= startOfYearCiclye && e.Date < endOfYearCiclye)
                .OrderByDescending(e => e.Date).ToListAsync();
            var yearlyUserData = await _context.YearlyUserData
                .Where(e => e.Id == userId && e.Date.Year== DateTime.Now.Year)
                .OrderBy(e => e.Date).ToListAsync();

            UserDashboard userDashboard = new()
            {
                DailyUserData = dailyUserData,
                MonthlyUserData = monthlyUserData,
                YearlyUserData = yearlyUserData,
                RecentExpenses = expenses
            };

            var jsonStringDaily = JsonSerializer.Serialize(dailyUserData);

            List<int> recentMonths = new List<int>();
            List<float> recentMonthsTotal = new List<float>();
            foreach (var item in monthlyUserData)
            {
                if (item.Date.Month > DateTime.Now.Month - 5 && item.Date.Month <= DateTime.Now.Month)
                {
                    recentMonths.Add(item.Date.Month);
                    recentMonthsTotal.Add(item.MonthlyIncome - item.MonthlyExpense);

                }
                else
                {
                    recentMonths.Add(-1);
                    recentMonthsTotal.Add(-1);
                }
            }
            var jsonStringMonthly = JsonSerializer.Serialize(monthlyUserData);

            ViewBag.DailyUserDataJson = jsonStringDaily;
            ViewBag.RecentMonths = recentMonths;
            ViewBag.RecentMonthsTotal = recentMonthsTotal;
            ViewBag.MonthlyUserDataJson = jsonStringMonthly;
            ViewBag.YearlyTotal =userDashboard.YearlyUserData[0].YearlyIncome - userDashboard.YearlyUserData[0].YearlyExpense;
            ViewBag.DayOfCycleReset = dayOfCycleReset;
            return View(userDashboard);
        }
        private string GetUserId()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return userId;
        }
    }
}
