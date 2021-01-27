using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;
using Spice.Models.ViewModel;

namespace Spice.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SubCategoryController : Controller
    {
        private readonly ApplicationDbContext _db;
        [TempData]
        public string StatusMessage { get; set; }

        public SubCategoryController(ApplicationDbContext db)
        {
            _db = db;
        }
        //Get-Indesx
        public async Task<IActionResult> Index()
        {
            var subCategory = await _db.SubCategory.Include(s=> s.Category).ToListAsync();
            return View(subCategory);
        }

        //Get-Create
        public async Task<IActionResult> Create()
        {
            SubCategoryAndCategoryViewModel model = new SubCategoryAndCategoryViewModel()
            {

                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = new Models.SubCategory(),
                SubCategoryList = _db.SubCategory.OrderBy(p => p.Name).Select(p => p.Name).Distinct().ToList()
            };

            return View(model);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubCategoryAndCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var doesSubCategoryExist = _db.SubCategory.Include(p => p.Category).Where(s => s.Name == model.SubCategory.Name && s.Category.Id == model.SubCategory.CategoryId);

                if(doesSubCategoryExist.Count()>0)
                {
                    StatusMessage = "Error: Sub Category already exist in " + doesSubCategoryExist.First().Category.Name + " category Please use another name";
                }
                else
                {
                    _db.SubCategory.Add(model.SubCategory);
                    await _db.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
            }

            SubCategoryAndCategoryViewModel SubVm = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = model.SubCategory,
                SubCategoryList = await _db.SubCategory.OrderBy(s => s.Name).Select(s => s.Name).ToListAsync(),
                StatusMessage = StatusMessage
            };

            return View(SubVm);

        }

        [ActionName("GEtSubCategory")]
        public async Task<IActionResult> GetSubCategory(int id)
        {
            List<SubCategory> subCategories = new List<SubCategory>();

            

            subCategories = await (from subcategory in _db.SubCategory
                                   where subcategory.CategoryId == id
                                   select subcategory).ToListAsync();

            return Json(new SelectList(subCategories, "Id", "Name"));
         }

        //Get-Edit
        public async Task<IActionResult> Edit(int ? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var subCategory = await _db.SubCategory.SingleOrDefaultAsync(s => s.Id == id);
            if (subCategory == null)
            {
                return NotFound();
            }

            SubCategoryAndCategoryViewModel model = new SubCategoryAndCategoryViewModel()
            {

                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = subCategory,
                SubCategoryList = _db.SubCategory.OrderBy(p => p.Name).Select(p => p.Name).Distinct().ToList()
            };

            return View(model);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit (SubCategoryAndCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var doesSubCategoryExist = _db.SubCategory.Include(p => p.Category).Where(s => s.Name == model.SubCategory.Name && s.Category.Id == model.SubCategory.CategoryId);

                if (doesSubCategoryExist.Count() > 0)
                {
                    StatusMessage = "Error: Sub Category already exist in " + doesSubCategoryExist.First().Category.Name + " category Please use another name";
                }
                else
                {
                    var EditedSubCategory =await _db.SubCategory.FindAsync(model.SubCategory.Id);
                    EditedSubCategory.Name = model.SubCategory.Name;
                    await _db.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
            }

            SubCategoryAndCategoryViewModel SubVm = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = model.SubCategory,
                SubCategoryList = await _db.SubCategory.OrderBy(s => s.Name).Select(s => s.Name).ToListAsync(),
                StatusMessage = StatusMessage
            };

            return View(SubVm);

        }

        //Get-Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var SubCategoryFromDb = await _db.SubCategory.Include(s => s.Category).FirstOrDefaultAsync(s => s.Id == id);
            if (SubCategoryFromDb == null)
            {
                return NotFound();
            }
            return View(SubCategoryFromDb);
        }

        //Get-Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var SubCategoryFromDb = await _db.SubCategory.Include(s => s.Category).FirstOrDefaultAsync(s => s.Id == id);
            if (SubCategoryFromDb == null)
            {
                return NotFound();
            }
            return View(SubCategoryFromDb);
        }

        //Post-Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmDelete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var SubCategoryFromDb = await _db.SubCategory.Include(s => s.Category).FirstOrDefaultAsync(s => s.Id == id);
            if (SubCategoryFromDb == null)
            {
                return NotFound();
            }

            _db.SubCategory.Remove(SubCategoryFromDb);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index)); 
        }

    }
}
