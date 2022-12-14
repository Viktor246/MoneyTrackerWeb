using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Areas.Identity.Data;
using MoneyTracker.Data;
using MoneyTracker.Models;
using MoneyTracker.Utility;
using NuGet.Protocol;
using System;
using System.Collections.Generic;
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


            var categoryList = await _context.Categories.Include(c => c.SubCategories).ThenInclude(sc => sc.Expenses)
                .Where(c => c.OwnerId == userId).OrderBy(c => c.DisplayOrder).ToListAsync();
            List<String> categoryNames = new();
            List<String> subCategoryNames = new();
            List<String> subCategoryCategoryNames = new();
            List<float> categoryExpenseSums = new();
            List<float> subCategoryExpenseSums = new();
            foreach (var category in categoryList)
            {
                float categorySum = 0;
                foreach (var subCategory in category.SubCategories)
                {
                    float subCategorySum = 0;
                    foreach (var expense in subCategory.Expenses)
                    {
                        if (expense.DateOfExpense >= minDateDaily && expense.DateOfExpense < maxDateDaily)
                        {
                            categorySum += expense.Value;
                            subCategorySum += expense.Value;
                        }
                    }
                    if (subCategorySum != 0)
                    {
                        subCategoryNames.Add(subCategory.Name);
                        subCategoryCategoryNames.Add(category.Name);
                        subCategoryExpenseSums.Add(subCategorySum);
                    }
                }
                if (categorySum != 0)
                {
                    categoryNames.Add(category.Name);
                    categoryExpenseSums.Add(categorySum);
                }
            }

            UserDashboard userDashboard = new()
            {
                DailyUserData = dailyUserData,
                MonthlyUserData = monthlyUserData,
                YearlyUserData = yearlyUserData,
                RecentExpenses = expenses
            };

            var jsonStringDaily = JsonSerializer.Serialize(dailyUserData);

            List<int> recentMonths = new();
            List<float> recentMonthsTotal = new();
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



            var lastBalance = _context.Balance.OrderByDescending(x => x.Date).Where(x => x.OwnerId == userId).FirstOrDefault();

            float currentBalance = 0;
            if (lastBalance != null)
            {
                currentBalance = lastBalance.Value;
                var allExpenses = _context.Expense.Where(e => e.DateOfExpense > lastBalance.Date && e.DateOfExpense < DateTime.Now && e.OwnerId == userId);
                foreach (var expense in allExpenses)
                {
                    currentBalance -= expense.Value;
                }
                var allIncomes = _context.Income.Where(i => i.Date > lastBalance.Date && i.Date < DateTime.Now && i.OwnerId == userId);
                foreach (var income in allIncomes)
                {
                    currentBalance += income.Value;
                }
            }

            currentBalance = (float)Math.Round(currentBalance, 2);

            ViewBag.DailyUserDataJson = jsonStringDaily;
            ViewBag.RecentMonths = recentMonths;
            ViewBag.RecentMonthsTotal = recentMonthsTotal;
            ViewBag.MonthlyUserDataJson = jsonStringMonthly;
            ViewBag.YearlyTotal =userDashboard.YearlyUserData[0].YearlyIncome - userDashboard.YearlyUserData[0].YearlyExpense;
            ViewBag.DayOfCycleReset = dayOfCycleReset;
            ViewBag.categoryNames = categoryNames;
            ViewBag.categoryExpenseSums = categoryExpenseSums;
            ViewBag.SubCategoryNames = subCategoryNames;
            ViewBag.SubCategoryCategoryNames = subCategoryCategoryNames;
            ViewBag.SubCategoryExpenseSums = subCategoryExpenseSums;
            ViewBag.CurrentBalance = currentBalance;


            return View(userDashboard);
        }
        private string GetUserId()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return userId;
        }
    }
}
