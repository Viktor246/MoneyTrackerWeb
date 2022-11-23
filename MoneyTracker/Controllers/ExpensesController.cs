using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Data;
using MoneyTracker.Models;
using MoneyTracker.Utility;

namespace MoneyTracker.Controllers
{
    public class ExpensesController : Controller
    {
        private readonly ApplicationDBContext _context;
        private readonly IAuthorizationService _authorizationService;

        public ExpensesController(ApplicationDBContext context, IAuthorizationService authorizationService)
        {
            _context = context;
            _authorizationService = authorizationService;
        }

        // GET: Expenses
        public async Task<IActionResult> Index(string sortOrder = "", string Filters = "", string searchString = "", int pageSize = 10, int page = 1)
        {
            ViewBag.CurrentSortOrder = sortOrder;
            ViewBag.CurrentFilters = Filters;
            ViewBag.CurrentSearchString = searchString;
            var applicationDBContext = _context.Expense.Include(e => e.SubCategory).Where(e => e.OwnerId == this.getUserId());
            if (pageSize <= 0)
            {
                pageSize = 10;
            }
            if (page <= 0)
            {
                page = 1;
            }

            List<SelectListItem> filters = new List<SelectListItem>();
            filters.Add(new SelectListItem { Text = "--Select option--", Value = "" });
            filters.Add(new SelectListItem { Text = "Description", Value = "Description" });
            filters.Add(new SelectListItem { Text = "Sub category", Value = "Sub category" });
            filters.Add(new SelectListItem { Text = "Value", Value = "Value" });
            filters.Add(new SelectListItem { Text = "Date", Value = "Date" });
            ViewBag.Filters = filters;
            ViewBag.DateSortParam = String.IsNullOrEmpty(sortOrder) ? "date_desc" : "";
            ViewBag.DescriptionSortParam = sortOrder == "desc" ? "desc_desc" : "desc";
            ViewBag.ValueSortParam = sortOrder == "value" ? "value_desc" : "value";
            ViewBag.SubCatSortParam = sortOrder == "subcat" ? "subcat_desc" : "subcat";

            if (!String.IsNullOrEmpty(Filters))
            {
                if (!String.IsNullOrEmpty(searchString))
                {
                    switch (Filters)
                    {
                        case "Description":
                            applicationDBContext = applicationDBContext.Where(s => s.Description.Contains(searchString));
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

            switch (sortOrder)
            {
                case "date_desc":
                    applicationDBContext = applicationDBContext.OrderByDescending(s => s.DateOfExpense);
                    break;
                case "desc_desc":
                    applicationDBContext = applicationDBContext.OrderByDescending(s => s.Description);
                    break;
                case "desc":
                    applicationDBContext = applicationDBContext.OrderBy(s => s.Description);
                    break;
                case "value_desc":
                    applicationDBContext = applicationDBContext.OrderByDescending(s => s.Value);
                    break;
                case "value":
                    applicationDBContext = applicationDBContext.OrderBy(s => s.Value);
                    break;
                case "subcat_desc":
                    applicationDBContext = applicationDBContext.OrderByDescending(s => s.SubCategoryId);
                    break;
                case "subcat":
                    applicationDBContext = applicationDBContext.OrderBy(s => s.SubCategoryId);
                    break;
                default:
                    applicationDBContext = applicationDBContext.OrderBy(s => s.DateOfExpense);
                    break;
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
            var userId = this.getUserId();
            var subcategories = _context.SubCategory.Where(c => c.OwnerId == userId);
            var categories = _context.Categories.Where(c => c.OwnerId == userId).OrderBy(c => c.DisplayOrder);
            ViewData["categories"] = new SelectList(categories, "Id", "Name");
            ViewBag.Now = DateTime.Now;
            return View();
        }

        [HttpPost]
        public IActionResult GetSubCategories(int categoryId)
        {

            var subCategories = _context.SubCategory.Where(c => c.CategoryId == categoryId && c.OwnerId == this.getUserId()).OrderBy(c => c.DisplayOrder);

            return new JsonResult(subCategories);
        }

        // POST: Expenses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Description,Value,DateOfExpense,SubCategoryId,RecordStatus,RecordStatusDate")] Expense expense)
        {

            var userId = this.getUserId();
            expense.SubCategory = await _context.SubCategory.FirstOrDefaultAsync(p => p.SubCategoryId == expense.SubCategoryId && p.OwnerId == userId);
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
            var categories = _context.Categories.Where(c => c.OwnerId == userId);
            ViewData["categories"] = new SelectList(categories, "Id", "Name");
            ViewBag.Now = DateTime.Now;
            return View(expense);
        }
        /*
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

            var result = await _authorizationService.AuthorizeAsync(User, expense, "isOwner");
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
            ViewData["SubCategoryId"] = new SelectList(_context.SubCategory, "SubCategoryId", "Name", expense.SubCategoryId);
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
        */
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
        private string getUserId()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return userId;
        }
    }
}
