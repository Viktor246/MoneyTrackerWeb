using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Data;
using MoneyTracker.Models;
using MoneyTracker.Utility;
using static NuGet.Packaging.PackagingConstants;

namespace MoneyTracker.Controllers
{
    public class SubCategoriesController : Controller
    {
        private readonly ApplicationDBContext _context;
        private readonly IAuthorizationService _authorizationService;

        public SubCategoriesController(ApplicationDBContext context, IAuthorizationService authorizationService)
        {
            _context = context;
            _authorizationService = authorizationService;

        }

        // GET: SubCategories/Index/5
        public async Task<IActionResult> Index(int? categoryId, string sortOrder = "", int pageSize = 10, int page = 1)
        {
            ViewBag.CurrentSortOrder = sortOrder;

            if (pageSize <= 0)
            {
                pageSize = 10;
            }
            if (page <= 0)
            {
                page = 1;
            }

            if (categoryId == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(categoryId);
            var result = await _authorizationService.AuthorizeAsync(User, category, "isOwner");
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

            ViewBag.CategoryId = categoryId;
            var applicationDBContext = _context.SubCategory.Include(s => s.Category).Where(s => s.CategoryId == categoryId && s.OwnerId == this.getUserId());

            ViewBag.DisplayOrderSortParam = String.IsNullOrEmpty(sortOrder) ? "display_order_desc" : "";
            ViewBag.DescriptionSortParam = sortOrder == "desc" ? "desc_desc" : "desc";
            ViewBag.NameSortParam = sortOrder == "name" ? "name_desc" : "name";
            ViewBag.CategorySortParam = sortOrder == "category" ? "category_desc" : "category";


            switch (sortOrder)
            {
                case "display_order_desc":
                    applicationDBContext = applicationDBContext.OrderByDescending(s => s.DisplayOrder);
                    break;
                case "desc_desc":
                    applicationDBContext = applicationDBContext.OrderByDescending(s => s.Description);
                    break;
                case "desc":
                    applicationDBContext = applicationDBContext.OrderBy(s => s.Description);
                    break;
                case "name_desc":
                    applicationDBContext = applicationDBContext.OrderByDescending(s => s.Name);
                    break;
                case "name":
                    applicationDBContext = applicationDBContext.OrderBy(s => s.Name);
                    break;
                case "category_desc":
                    applicationDBContext = applicationDBContext.OrderByDescending(s => s.CategoryId);
                    break;
                case "category":
                    applicationDBContext = applicationDBContext.OrderBy(s => s.CategoryId);
                    break;
                default:
                    applicationDBContext = applicationDBContext.OrderBy(s => s.DisplayOrder);
                    break;
            }

            int countOfItems = applicationDBContext.Count();
            int pageCount = countOfItems / pageSize;
            if (countOfItems % pageSize > 0)
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
        /*
        // GET: SubCategories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.SubCategory == null)
            {
                return NotFound();
            }

            var subCategory = await _context.SubCategory
                .Include(s => s.Category)
                .FirstOrDefaultAsync(m => m.SubCategoryId == id);
            if (subCategory == null)
            {
                return NotFound();
            }

            return View(subCategory);
        }
        */
        // GET: SubCategories/Create/5
        public async Task<IActionResult> Create(int? categoryId)
        {
            if (categoryId == null)
            {
                return NotFound();
            }
            var category = await _context.Categories.FindAsync(categoryId);
            var result = await _authorizationService.AuthorizeAsync(User, category, "isOwner");
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
            ViewBag.CategoryId = categoryId;
            return View();
        }

        // POST: SubCategories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SubCategoryId,Name,Description,DisplayOrder,CategoryId,RecordStatus,RecordStatusDate")] SubCategory subCategory)
        {
            var userId = this.getUserId();
            subCategory.Category = await _context.Categories.FirstOrDefaultAsync(p => p.Id == subCategory.CategoryId && p.OwnerId == userId);
            subCategory.OwnerId = userId;
            if (subCategory.Category == null)
            {
                ModelState.AddModelError("CategoryIsNull", "Selected category doesn't exist in database!");

            }
            ModelState.Remove("Category");
            ModelState.Remove("Expenses");
            if (ModelState.IsValid)
            {
                _context.Add(subCategory);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("DBError", e.InnerException.Message);
                    return View(subCategory);
                }
                TempData["success"] = "Sub Category created successfully!";
                return RedirectToAction(nameof(Index), new {categoryId = subCategory.CategoryId});
            }
            return View(subCategory);
        }

        // GET: SubCategories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.SubCategory == null)
            {
                return NotFound();
            }

            var subCategory = await _context.SubCategory.FindAsync(id);
            if (subCategory == null)
            {
                return NotFound();
            }
            var result = await _authorizationService.AuthorizeAsync(User, subCategory, "isOwner");
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
            ViewBag.CategoryId = subCategory.CategoryId;
            return View(subCategory);
        }

        // POST: SubCategories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SubCategoryId,Name,Description,DisplayOrder,CategoryId,RecordStatus,RecordStatusDate")] SubCategory subCategory)
        {
            ViewBag.CategoryId = subCategory.CategoryId;
            if (id != subCategory.SubCategoryId)
            {
                return NotFound();
            }
            subCategory.Category = await _context.Categories.FirstOrDefaultAsync(p => p.Id == subCategory.CategoryId);
            if (subCategory.Category == null)
            {
                ModelState.AddModelError("CategoryIsNull", "Selected category doesn't exist in database!");

            }
            subCategory.OwnerId = this.getUserId();

            ModelState.Remove("Category");
            ModelState.Remove("Expenses");
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(subCategory);
                    await _context.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("DBError", e.InnerException.Message);
                    return View(subCategory);
                }
                TempData["success"] = "Sub Category edited successfully!";
                return RedirectToAction(nameof(Index), new { categoryId = subCategory.CategoryId });
            }
            return View(subCategory);
        }

        // GET: SubCategories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.SubCategory == null)
            {
                return NotFound();
            }

            var subCategory = await _context.SubCategory
                .Include(s => s.Category)
                .FirstOrDefaultAsync(m => m.SubCategoryId == id);
            if (subCategory == null)
            {
                return NotFound();
            }
            var result = await _authorizationService.AuthorizeAsync(User, subCategory, "isOwner");
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

            ViewBag.CategoryId = subCategory.CategoryId;
            return View(subCategory);
        }

        // POST: SubCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.SubCategory == null)
            {
                return Problem("Entity set 'ApplicationDBContext.SubCategory'  is null.");
            }
            var subCategory = await _context.SubCategory.FindAsync(id);

            var result = await _authorizationService.AuthorizeAsync(User, subCategory, "isOwner");
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

            if (subCategory != null)
            {
                _context.SubCategory.Remove(subCategory);
            }
            
            await _context.SaveChangesAsync();
            TempData["success"] = "Sub Category deleted successfully!";
            return RedirectToAction(nameof(Index), new { categoryId = subCategory.CategoryId });
        }

        private bool SubCategoryExists(int id)
        {
          return _context.SubCategory.Any(e => e.SubCategoryId == id);
        }
        private string getUserId()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return userId;
        }
    }
}
