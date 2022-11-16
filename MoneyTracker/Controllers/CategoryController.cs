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

        public CategoryController(ApplicationDBContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            IEnumerable<Category> objectCategoryList = _db.Categories.Where(c => c.OwnerId == this.getUserId());
            return View(objectCategoryList);
        }
        //GET
        public IActionResult Edit(int? id)
        {
            if ( id == null || id == 0)
            {
                return NotFound();
            }

            var categoryFromDb = _db.Categories.Find(id);

            if (categoryFromDb == null)
            {
                return NotFound();
            }

            return View(categoryFromDb);
        }
        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category obj)
        {
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
                    _db.SaveChanges();
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
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var categoryFromDb = _db.Categories.Find(id);

            if (categoryFromDb == null)
            {
                return NotFound();
            }

            return View(categoryFromDb);
        }
        //POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePOST(int? id)
        {
            var obj = _db.Categories.Find(id);
            if (obj == null)
            {
                return NotFound();
            }
            _db.Categories.Remove(obj);
            try
            {
                _db.SaveChanges();
            }
            catch (Exception e)
            {
                ModelState.AddModelError("DBError", e.InnerException.Message);
                return View(obj);
            }
            TempData["success"] = "Category deleted successfully!";
            return RedirectToAction("Index");
        }

        public string getUserId()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return userId;
        }
    }
}
