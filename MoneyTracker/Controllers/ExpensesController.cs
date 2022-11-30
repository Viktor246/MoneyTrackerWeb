using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Areas.Identity.Data;
using MoneyTracker.Data;
using MoneyTracker.Models;
using MoneyTracker.Utility;

namespace MoneyTracker.Controllers
{
    public class ExpensesController : Controller
    {
        private readonly ApplicationDBContext _context;
        private readonly IAuthorizationService _authorizationService;
        private readonly ApplicationUserContext _userContext;

        public ExpensesController(ApplicationDBContext context, IAuthorizationService authorizationService, ApplicationUserContext userContext)
        {
            _context = context;
            _authorizationService = authorizationService;
            _userContext = userContext;
        }

        // GET: Expenses
        public async Task<IActionResult> Index(string sortOrder = "", string Filters = "", string searchString = "", int pageSize = 10, int page = 1, int cycle = 0, int year = 0)
        {
            ViewBag.CurrentSortOrder = sortOrder;
            ViewBag.CurrentFilters = Filters;
            ViewBag.CurrentSearchString = searchString;

            var user = await _userContext.Users.FindAsync(this.GetUserId());
            if (user == null)
            {
                return LocalRedirect("/Home/Identity/Login");
            }
            var applicationDBContext = _context.Expense.Include(e => e.SubCategory).Where(e => e.OwnerId == this.GetUserId());

            if (cycle == 0 || cycle > 12)
            {
                cycle = DateTime.Now.Month;
            }
            if (year == 0)
            {
                year = DateTime.Now.Year;
            }
            DateTime minDate = new(year, cycle, user.DayOfCycleReset);

            ViewBag.Month = cycle;

            if (cycle == 12)
            {
                cycle = 1;
                year++;
            } else
            {
                cycle++;
            }
            DateTime maxDate = new(year, cycle, user.DayOfCycleReset);

            var minYearExpense = applicationDBContext.OrderBy(e => e.DateOfExpense).FirstOrDefault();
            int minYear;
            if (minYearExpense == null)
            {
                minYear = DateTime.Now.Year;
            }
            else
            {
                minYear = minYearExpense.DateOfExpense.Year;
            }

            var maxYearExpense = applicationDBContext.OrderByDescending(e => e.DateOfExpense).FirstOrDefault();
            int maxYear;
            if (maxYearExpense == null)
            {
                maxYear = DateTime.Now.Year;
            }
            else
            {
                maxYear = maxYearExpense.DateOfExpense.Year;
            }

            List<SelectListItem> years = new();
            for (int i = minYear; i <= maxYear; i++)
            {
                if (i == year)
                {
                    years.Add(new SelectListItem { Text = "" + i, Value = i.ToString(), Selected = true });
                }
                else
                {
                    years.Add(new SelectListItem { Text = "" + i, Value = i.ToString(), Selected = false });
                }
            }
            ViewBag.Years = years;

            applicationDBContext = applicationDBContext.Where(e => e.DateOfExpense > minDate && e.DateOfExpense < maxDate);

            if (!String.IsNullOrEmpty(Filters))
            {
                if (!String.IsNullOrEmpty(searchString))
                {
                    switch (Filters)
                    {
                        case "Description":
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                            applicationDBContext = applicationDBContext.Where(predicate: s => s.Description.Contains(searchString));
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                            break;
                        case "Sub category":
                            applicationDBContext = applicationDBContext.Where(s => s.SubCategory.Name.Contains(searchString));
                            break;
                        case "Value":
                            float number;
                            if (!float.TryParse(searchString, out number)) { TempData["error"] = "Invalid number format!"; break; }
                            applicationDBContext = applicationDBContext.Where(s => s.Value == number);
                            break;
                        case "Date":
                            DateTime date;
                            if (!DateTime.TryParse(searchString, out date)) { TempData["error"] = "Invalid date format!"; break; }
                            applicationDBContext = applicationDBContext.Where(s => s.DateOfExpense == date);
                            break;
                        default:
                            break;
                    }

                }
            }

            List<SelectListItem> filters = new()
            {
                new SelectListItem { Text = "--Select option--", Value = "" },
                new SelectListItem { Text = "Description", Value = "Description" },
                new SelectListItem { Text = "Sub category", Value = "Sub category" },
                new SelectListItem { Text = "Value", Value = "Value" },
                new SelectListItem { Text = "Date", Value = "Date" }
            };
            ViewBag.Filters = filters;
            ViewBag.DateSortParam = String.IsNullOrEmpty(sortOrder) ? "date_desc" : "";
            ViewBag.DescriptionSortParam = sortOrder == "desc" ? "desc_desc" : "desc";
            ViewBag.ValueSortParam = sortOrder == "value" ? "value_desc" : "value";
            ViewBag.SubCatSortParam = sortOrder == "subcat" ? "subcat_desc" : "subcat";

            applicationDBContext = sortOrder switch
            {
                "date_desc" => applicationDBContext.OrderByDescending(s => s.DateOfExpense),
                "desc_desc" => applicationDBContext.OrderByDescending(s => s.Description),
                "desc" => applicationDBContext.OrderBy(s => s.Description),
                "value_desc" => applicationDBContext.OrderByDescending(s => s.Value),
                "value" => applicationDBContext.OrderBy(s => s.Value),
                "subcat_desc" => applicationDBContext.OrderByDescending(s => s.SubCategoryId),
                "subcat" => applicationDBContext.OrderBy(s => s.SubCategoryId),
                _ => applicationDBContext.OrderBy(s => s.DateOfExpense),
            };
            if (pageSize <= 0)
            {
                pageSize = 10;
            }
            if (page <= 0)
            {
                page = 1;
            }
            int countOfExpenses = applicationDBContext.Count();
            int pageCount = countOfExpenses / pageSize;
            if (countOfExpenses % pageSize > 0)
            {
                pageCount++;
            }

            ViewBag.Pages = Utilities.getPaginationList(page, pageCount);
            ViewBag.Page = page.ToString();
            ViewBag.PageSize = pageSize.ToString();
            ViewBag.PageCount = pageCount.ToString();

            if (page == 1)
            {
                ViewBag.FirstPage = "disabled";

            } else
            {
                ViewBag.FirstPage = "";
            }
            if (page == pageCount)
            {
                ViewBag.LastPage = "disabled";

            }
            else
            {
                ViewBag.LastPage = "";
            }
            return View(await applicationDBContext.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync());
        }

        /*
        // GET: Expenses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Expense == null)
            {
                return NotFound();
            }

            var expense = await _context.Expense
                .Include(e => e.Category)
                .Include(e => e.SubCategory)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (expense == null)
            {
                return NotFound();
            }

            return View(expense);
        }
        */
        // GET: Expenses/Create
        public IActionResult Create()
        {
            var userId = this.GetUserId();
            var subcategories = _context.SubCategory.Where(c => c.OwnerId == userId);
            var categories = _context.Categories.Where(c => c.OwnerId == userId).OrderBy(c => c.DisplayOrder);
            ViewData["categories"] = new SelectList(categories, "Id", "Name");
            ViewBag.Now = DateTime.Now;
            return View();
        }

        [HttpPost]
        public IActionResult GetSubCategories(int categoryId)
        {

            var subCategories = _context.SubCategory.Where(c => c.CategoryId == categoryId && c.OwnerId == this.GetUserId()).OrderBy(c => c.DisplayOrder);

            return new JsonResult(subCategories);
        }

        // POST: Expenses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Description,Value,DateOfExpense,SubCategoryId,RecordStatus,RecordStatusDate")] Expense expense)
        {

            var userId = this.GetUserId();
            if (userId == null)
            {
                return LocalRedirect("/Identity/Account/Login");
            }
            var categories = _context.Categories.Where(c => c.OwnerId == userId);

            SubCategory? subCategory = await _context.SubCategory.FirstOrDefaultAsync(p => p.SubCategoryId == expense.SubCategoryId && p.OwnerId == userId);
            expense.SubCategory = subCategory;
            expense.OwnerId = userId;
            if (expense.SubCategory == null)
            {
                ModelState.AddModelError("SubCategoryIsNull", "Selected sub category doesn't exist in database!");

            }
            ModelState.Remove("SubCategory");
            ModelState.Remove("SubCategoryId");
            if (ModelState.IsValid)
            {
                _context.Add(expense);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["categories"] = new SelectList(categories, "Id", "Name");
            ViewBag.Now = DateTime.Now;
            return View(expense);
        }

        // GET: Expenses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Expense == null)
            {
                return NotFound();
            }

            var expense = await _context.Expense.FindAsync(id);
            if (expense == null)
            {
                return NotFound();
            }

            _context.Entry(expense)
                .Reference(e => e.SubCategory)
                .Load();
            _context.Entry(expense.SubCategory).Reference(e => e.Category).Load();

            var result = await _authorizationService.AuthorizeAsync(User, expense, "isOwner");
            if (User.Identity == null)
            {
                return LocalRedirect("/Home/Identity/Login");
            }
            if (!result.Succeeded)
            {
                if (User.Identity.IsAuthenticated)
                {
                    return new ForbidResult();
                }
                else
                {
                    return new ChallengeResult();
                }
            }
            ViewBag.Categories = new SelectList(_context.Categories.Where(c => c.OwnerId == expense.OwnerId), "Id", "Name", expense.SubCategory.Category.Id);
            ViewData["SubCategoryId"] = new SelectList(_context.SubCategory.Where(c => c.OwnerId == expense.OwnerId && c.CategoryId == expense.SubCategory.Category.Id), "SubCategoryId", "Name", expense.SubCategoryId);
            return View(expense);
        }

        // POST: Expenses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Description,Value,DateOfExpense,SubCategoryId,OwnerId,RecordStatus,RecordStatusDate")] Expense expense)
        {
            if (id != expense.Id)
            {
                return NotFound();
            }

            SubCategory? subCategory = await _context.SubCategory.FirstOrDefaultAsync(p => p.SubCategoryId == expense.SubCategoryId && p.OwnerId == expense.OwnerId);
            if (subCategory == null)
            {
                return NotFound();
            }
            expense.SubCategory = subCategory;
            if (expense.SubCategory == null)
            {
                ModelState.AddModelError("SubCategoryIsNull", "Selected sub category doesn't exist in database!");

            }
            ModelState.Remove("SubCategory");
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(expense);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ExpenseExists(expense.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["SubCategoryId"] = new SelectList(_context.SubCategory, "SubCategoryId", "Name", expense.SubCategoryId);
            return View(expense);
        }
        
        // GET: Expenses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Expense == null)
            {
                return NotFound();
            }

            var expense = await _context.Expense
                .Include(e => e.SubCategory)
                .ThenInclude (sc => sc.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (expense == null)
            {
                return NotFound();
            }

            var result = await _authorizationService.AuthorizeAsync(User, expense, "isOwner");
            if (User.Identity == null)
            {
                return LocalRedirect("/Home/Identity/Login");
            }
            if (!result.Succeeded)
            {
                if (User.Identity.IsAuthenticated)
                {
                    return new ForbidResult();
                }
                else
                {
                    return new ChallengeResult();
                }
            }
            ViewBag.Category = expense.SubCategory.Category.Name;
            return View(expense);
        }

        // POST: Expenses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Expense == null)
            {
                return Problem("Entity set 'ApplicationDBContext.Expense'  is null.");
            }
            var expense = await _context.Expense.FindAsync(id);
            if (expense != null)
            {
                _context.Expense.Remove(expense);
            }
            var result = await _authorizationService.AuthorizeAsync(User, expense, "isOwner");
            if (User.Identity == null)
            {
                return LocalRedirect("/Home/Identity/Login");
            }
            if (!result.Succeeded)
            {
                if (User.Identity.IsAuthenticated)
                {
                    return new ForbidResult();
                }
                else
                {
                    return new ChallengeResult();
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ExpenseExists(int id)
        {
          return _context.Expense.Any(e => e.Id == id);
        }
        private string GetUserId()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return userId;
        }
    }
}
