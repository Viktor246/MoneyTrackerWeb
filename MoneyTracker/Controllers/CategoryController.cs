using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Data;
using MoneyTracker.Models;
using MoneyTracker.Utility;
using System.Security.Claims;

namespace MoneyTracker.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDBContext _db;
        private readonly IAuthorizationService _authorizationService;

        public CategoryController(ApplicationDBContext db, IAuthorizationService authorizationService)
        {
            _db = db;
            _authorizationService = authorizationService;
        }

        public async Task<IActionResult> Index(string sortOrder = "", int pageSize = 10, int page = 1)
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
            var objectCategoryList = _db.Categories.Include(c => c.SubCategories).Where(c => c.OwnerId == this.GetUserId());
            
            ViewBag.DisplayOrderSortParam = String.IsNullOrEmpty(sortOrder) ? "display_order_desc" : "";
            ViewBag.DescriptionSortParam = sortOrder == "desc" ? "desc_desc" : "desc";
            ViewBag.NameSortParam = sortOrder == "name" ? "name_desc" : "name";

            objectCategoryList = sortOrder switch
            {
                "display_order_desc" => objectCategoryList.OrderByDescending(s => s.DisplayOrder),
                "desc_desc" => objectCategoryList.OrderByDescending(s => s.Description),
                "desc" => objectCategoryList.OrderBy(s => s.Description),
                "name_desc" => objectCategoryList.OrderByDescending(s => s.Name),
                "name" => objectCategoryList.OrderBy(s => s.Name),
                _ => objectCategoryList.OrderBy(s => s.DisplayOrder),
            };
            int countOfItems = objectCategoryList.Count();
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
            return View(await objectCategoryList.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync());

        }
        //GET
        public async Task<IActionResult> Edit(int? id)
        {
            if ( id == null || id == 0)
            {
                return NotFound();
            }

            var categoryFromDb = await _db.Categories.FindAsync(id);

            if (categoryFromDb == null || categoryFromDb.OwnerId == null)
            {
                return NotFound();
            }

            var result = await _authorizationService.AuthorizeAsync(User, categoryFromDb, "isOwner");
            if (User.Identity == null)
            {
                return LocalRedirect("/Identity/Account/Login");
            }
            if (!result.Succeeded)
            {
                return User.Identity.IsAuthenticated ? new ForbidResult() : new ChallengeResult();
            }
            return View(categoryFromDb);
        }
        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Category obj)
        {
            if (obj.Name == obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError("NameDisOrderMatch", "The Dispaly Order can not exactly match the Name of category.");
            }
            obj.OwnerId = this.GetUserId();
            ModelState.Remove("SubCategories");
            ModelState.Remove("Expenses");
            if (ModelState.IsValid)
            {
                obj.RecordStatus = 2;
                obj.RecordStatusDate = DateTime.Now;
                _db.Categories.Update(obj);

                try
                {
                    await _db.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    if (e.InnerException == null)
                    {
                        ModelState.AddModelError("DBError", errorMessage: "Unknown error");
                    }
                    else
                    {
                        ModelState.AddModelError("DBError", errorMessage: e.InnerException.Message);

                    }
                    return View(obj);
                }
                TempData["success"] = "Category edited successfully!";
                return RedirectToAction("Index");
            }
            return View(obj);
        }
        //GET
        public IActionResult Create()
        {
            return View();
        }
        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category obj)
        {
            obj.OwnerId = this.GetUserId();
            if (obj.Name == obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError("NameDisOrderMatch", "The Dispaly Order can not exactly match the Name of category.");
            }
            ModelState.Remove("SubCategories");
            if (ModelState.IsValid)
            {
                _db.Categories.Add(obj);
                try {
                _db.SaveChanges();
                }
                catch(Exception e) {
                    if (e.InnerException == null)
                    {
                        ModelState.AddModelError("DBError", errorMessage: "Unknown error");
                    }
                    else
                    {
                        ModelState.AddModelError("DBError", errorMessage: e.InnerException.Message);

                    }
                    return View(obj);
                }
                TempData["success"] = "Category created successfully!";
                return RedirectToAction("Index");
            }
            return View(obj);
        }
        //GET
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var categoryFromDb = await _db.Categories.FindAsync(id);

            if (categoryFromDb == null)
            {
                return NotFound();
            }

            var result = await _authorizationService.AuthorizeAsync(User, categoryFromDb, "isOwner");

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
            return View(categoryFromDb);
        }
        //POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePOST(int? id)
        {
            var obj = await _db.Categories.FindAsync(id);
            if (obj == null)
            {
                return NotFound();
            }

            var result = await _authorizationService.AuthorizeAsync(User, obj, "isOwner");
            if (User.Identity == null)
            {
                return LocalRedirect("/Home/Identity/Redirect");
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

            _db.Categories.Remove(obj);
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                if (e.InnerException == null)
                {
                    ModelState.AddModelError("DBError", "Unknown");
                }
                else
                {
                    ModelState.AddModelError("DBError", e.InnerException.Message);
                }
                return View(obj);
            }
            TempData["success"] = "Category deleted successfully!";
            return RedirectToAction("Index");
        }

        private string GetUserId()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return userId;
        }
    }
}
