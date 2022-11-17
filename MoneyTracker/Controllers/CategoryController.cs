using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Data;
using MoneyTracker.Models;
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

        public IActionResult Index()
        {
            IEnumerable<Category> objectCategoryList = _db.Categories.Where(c => c.OwnerId == this.getUserId());
            return View(objectCategoryList);
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Category obj)
        {
            var result = await _authorizationService.AuthorizeAsync(User, obj, "isOwner");
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
            if (obj.Name == obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError("NameDisOrderMatch", "The Dispaly Order can not exactly match the Name of category.");
            }
            ModelState.Remove("SubCategories");
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
                    ModelState.AddModelError("DBError", e.InnerException.Message);
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
            obj.OwnerId = this.getUserId();
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
                    ModelState.AddModelError("DBError", e.InnerException.Message);
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
                ModelState.AddModelError("DBError", e.InnerException.Message);
                return View(obj);
            }
            TempData["success"] = "Category deleted successfully!";
            return RedirectToAction("Index");
        }

        private string getUserId()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return userId;
        }
    }
}
