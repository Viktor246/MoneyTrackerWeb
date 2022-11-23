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
    public class IncomesController : Controller
    {
        private readonly ApplicationDBContext _context;
        private readonly IAuthorizationService _authorizationService;
        private readonly ApplicationUserContext _userContext;

        public IncomesController(ApplicationDBContext context, IAuthorizationService authorizationService, ApplicationUserContext userContext)
        {
            _context = context;
            _authorizationService = authorizationService;
            _userContext = userContext;
        }

        // GET: Incomes
        public async Task<IActionResult> Index(string sortOrder = "", string Filters = "", string searchString = "", int pageSize = 10, int page = 1, int cycle = 0, int year = 0)
        {
            ViewBag.CurrentSortOrder = sortOrder;
            ViewBag.CurrentFilters = Filters;
            ViewBag.CurrentSearchString = searchString;

            var user = await _userContext.Users.FindAsync(this.getUserId());
            var applicationDBContext = _context.Income.Where(e => e.OwnerId == this.getUserId());

            if (cycle == 0 || cycle > DateTime.Now.Month)
            {
                cycle = DateTime.Now.Month;
            }
            if (year == 0)
            {
                year = DateTime.Now.Year;
            }

            DateTime minDate = new DateTime(year, cycle, user.DayOfCycleReset);

            ViewBag.Month = cycle;

            if (cycle == 12)
            {
                cycle = 1;
                year++;
            }
            else
            {
                cycle++;
            }
            DateTime maxDate = new DateTime(year, cycle, user.DayOfCycleReset);

            var minYearIncome = applicationDBContext.OrderBy(e => e.Date).FirstOrDefault();

            List<SelectListItem> years = new List<SelectListItem>();
            for (int i = minYearIncome.Date.Year; i <= DateTime.Now.Year; i++)
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

            applicationDBContext = applicationDBContext.Where(e => e.Date > minDate && e.Date < maxDate);

            if (!String.IsNullOrEmpty(Filters))
            {
                if (!String.IsNullOrEmpty(searchString))
                {
                    switch (Filters)
                    {
                        case "Description":
                            applicationDBContext = applicationDBContext.Where(s => s.Description.Contains(searchString));
                            break;
                        case "Value":
                            float number;
                            if (!float.TryParse(searchString, out number)) { TempData["error"] = "Invalid number format!"; break; }
                            applicationDBContext = applicationDBContext.Where(s => s.Value == number);
                            break;
                        case "Date":
                            DateTime date;
                            if (!DateTime.TryParse(searchString, out date)) { TempData["error"] = "Invalid date format!"; break; }
                            applicationDBContext = applicationDBContext.Where(s => s.Date == date);
                            break;
                        default:
                            break;
                    }

                }
            }

            List<SelectListItem> filters = new List<SelectListItem>();
            filters.Add(new SelectListItem { Text = "--Select option--", Value = "" });
            filters.Add(new SelectListItem { Text = "Description", Value = "Description" });
            filters.Add(new SelectListItem { Text = "Value", Value = "Value" });
            filters.Add(new SelectListItem { Text = "Date", Value = "Date" });
            ViewBag.Filters = filters;
            ViewBag.DateSortParam = String.IsNullOrEmpty(sortOrder) ? "date_desc" : "";
            ViewBag.DescriptionSortParam = sortOrder == "desc" ? "desc_desc" : "desc";
            ViewBag.ValueSortParam = sortOrder == "value" ? "value_desc" : "value";

            switch (sortOrder)
            {
                case "date_desc":
                    applicationDBContext = applicationDBContext.OrderByDescending(s => s.Date);
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
                default:
                    applicationDBContext = applicationDBContext.OrderBy(s => s.Date);
                    break;
            }

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

            }
            else
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

        // GET: Incomes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Income == null)
            {
                return NotFound();
            }

            var income = await _context.Income
                .FirstOrDefaultAsync(m => m.Id == id);
            if (income == null)
            {
                return NotFound();
            }

            return View(income);
        }

        // GET: Incomes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Incomes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Description,Value,Date,OwnerId,RecordStatus,RecordStatusDate")] Income income)
        {
            if (ModelState.IsValid)
            {
                _context.Add(income);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(income);
        }

        // GET: Incomes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Income == null)
            {
                return NotFound();
            }

            var income = await _context.Income.FindAsync(id);
            if (income == null)
            {
                return NotFound();
            }
            return View(income);
        }

        // POST: Incomes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Description,Value,Date,OwnerId,RecordStatus,RecordStatusDate")] Income income)
        {
            if (id != income.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(income);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!IncomeExists(income.Id))
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
            return View(income);
        }

        // GET: Incomes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Income == null)
            {
                return NotFound();
            }

            var income = await _context.Income
                .FirstOrDefaultAsync(m => m.Id == id);
            if (income == null)
            {
                return NotFound();
            }

            return View(income);
        }

        // POST: Incomes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Income == null)
            {
                return Problem("Entity set 'ApplicationDBContext.Income'  is null.");
            }
            var income = await _context.Income.FindAsync(id);
            if (income != null)
            {
                _context.Income.Remove(income);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool IncomeExists(int id)
        {
          return _context.Income.Any(e => e.Id == id);
        }
        private string getUserId()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return userId;
        }
    }
}
