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
    public class BalancesController : Controller
    {
        private readonly ApplicationDBContext _context;
        private readonly IAuthorizationService _authorizationService;
        private readonly ApplicationUserContext _userContext;
        public BalancesController(ApplicationDBContext context, IAuthorizationService authorizationService, ApplicationUserContext userContext)
        {
            _context = context;
            _authorizationService = authorizationService;
            _userContext = userContext;
        }

        // GET: Balances
        public async Task<IActionResult> Index(int pageSize = 10, int page = 1)
        {
            var user = await _userContext.Users.FindAsync(this.GetUserId());
            if (user == null)
            {
                return NotFound();
            }
            var applicationDBContext = _context.Balance.Where(e => e.OwnerId == this.GetUserId()).OrderByDescending(e => e.Date);

            if (pageSize <= 0)
            {
                pageSize = 10;
            }
            if (page <= 0)
            {
                page = 1;
            }
            int countOfIncomes = applicationDBContext.Count();
            int pageCount = countOfIncomes / pageSize;
            if (countOfIncomes % pageSize > 0)
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

        // GET: Balances/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Balance == null)
            {
                return NotFound();
            }

            var balance = await _context.Balance
                .FirstOrDefaultAsync(m => m.Id == id);
            if (balance == null)
            {
                return NotFound();
            }

            return View(balance);
        }

        // GET: Balances/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Balances/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Description,Value,Date,OwnerId,RecordStatus,RecordStatusDate")] Balance balance)
        {
            var userId = this.GetUserId();
            if (userId == null)
            {
                return LocalRedirect("/Identity/Account/Login");
            }
            balance.OwnerId = userId;
            if (ModelState.IsValid)
            {
                _context.Add(balance);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(balance);
        }

        // GET: Balances/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Balance == null)
            {
                return NotFound();
            }

            var balance = await _context.Balance.FindAsync(id);
            if (balance == null)
            {
                return NotFound();
            }
            return View(balance);
        }

        // POST: Balances/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Description,Value,Date,OwnerId,RecordStatus,RecordStatusDate")] Balance balance)
        {
            if (id != balance.Id)
            {
                return NotFound();
            }
            var userId = this.GetUserId();
            if (userId == null)
            {
                return LocalRedirect("/Identity/Account/Login");
            }
            balance.OwnerId = userId;
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(balance);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BalanceExists(balance.Id))
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
            return View(balance);
        }

        // GET: Balances/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Balance == null)
            {
                return NotFound();
            }

            var balance = await _context.Balance
                .FirstOrDefaultAsync(m => m.Id == id);
            if (balance == null)
            {
                return NotFound();
            }

            return View(balance);
        }

        // POST: Balances/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Balance == null)
            {
                return Problem("Entity set 'ApplicationDBContext.Balance'  is null.");
            }
            var balance = await _context.Balance.FindAsync(id);
            if (balance != null)
            {
                _context.Balance.Remove(balance);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BalanceExists(int id)
        {
          return _context.Balance.Any(e => e.Id == id);
        }
        private string GetUserId()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return userId;
        }
    }
}
