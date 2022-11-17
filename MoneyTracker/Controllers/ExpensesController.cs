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
        public async Task<IActionResult> Index()
        {
            var applicationDBContext = _context.Expense.Include(e => e.SubCategory).Where(e => e.OwnerId == this.getUserId());
            return View(await applicationDBContext.ToListAsync());
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
            var categories = _context.Categories.Where(c => c.OwnerId == userId);
            ViewData["categories"] = new SelectList(categories, "Id", "Name");
            ViewBag.Now = DateTime.Now;
            return View();
        }

        [HttpPost]
        public IActionResult GetSubCategories(int categoryId)
        {

            var subCategories = _context.SubCategory.Where(c => c.CategoryId == categoryId && c.OwnerId == this.getUserId());

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
