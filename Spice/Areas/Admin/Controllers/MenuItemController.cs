using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models.ViewModel;
using Spice.Utility;

namespace Spice.Areas.Admin.Controllers
{
   [Area("Admin")]
    public class MenuItemController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnviroment;

        [BindProperty]
        public MenuItemViewModel MenuItemVm { get; set; }

        public MenuItemController(ApplicationDbContext db, IWebHostEnvironment webHostEnviroment)
        {
            _db = db;
            _webHostEnviroment = webHostEnviroment;

            MenuItemVm = new MenuItemViewModel()
            {
                Category = _db.Category,
                MenuItem = new Models.MenuItem()
            };
        }
        public async Task <IActionResult> Index()
        {
            var menuItems = await _db.MenuItem.Include(s => s.Category).Include(s => s.SubCategory).ToListAsync();
            return View(menuItems);
        }

        //Get Create

        public IActionResult Create()
        {
            return View(MenuItemVm);
        }

        [HttpPost, ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePOST()
        {
            MenuItemVm.MenuItem.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());

            if (!ModelState.IsValid)
            {
                return View(MenuItemVm);
            }

           _db.MenuItem.Add(MenuItemVm.MenuItem);
            await _db.SaveChangesAsync();

            // working on the image saving section

            var webRootPath = _webHostEnviroment.WebRootPath;
            var files = HttpContext.Request.Form.Files;

            var MenuItemFromDb = await _db.MenuItem.FindAsync(MenuItemVm.MenuItem.Id);

            if (files.Count() > 0)
            {
                //file has been uploaded

                var uploads = Path.Combine(webRootPath, "images");
                var extension = Path.GetExtension(files[0].FileName);

                using (var filesStream = new FileStream(Path.Combine(uploads, MenuItemVm.MenuItem.Id + extension), FileMode.Create))
                {
                    files[0].CopyTo(filesStream);

                    MenuItemFromDb.Image = @"\images\" + MenuItemVm.MenuItem.Id + extension;

                }
            }

            else
            {
                // no file was uploaded, so use the default image

                var uploads = Path.Combine(webRootPath, @"images\" + SD.DefaultFoodImage);
                System.IO.File.Copy(uploads, webRootPath + @"\images\" + MenuItemVm.MenuItem.Id + ".png");
                MenuItemFromDb.Image= @"\images\"+ MenuItemVm.MenuItem.Id+".png";
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));


        }

        public async Task<IActionResult> Edit(int ? id)
        {
            if (id == null)
            {
           
                return NotFound();

            }

            MenuItemVm.MenuItem = await _db.MenuItem.Include(s => s.Category).Include(s => s.SubCategory).SingleOrDefaultAsync(s => s.Id == id);
            MenuItemVm.SubCategory = await _db.SubCategory.Where(s => s.CategoryId == MenuItemVm.MenuItem.CategoryId).ToListAsync();

            if (MenuItemVm.MenuItem == null)
            {
                return NotFound();
            }

            return View(MenuItemVm);
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPOST(int ? id)
        {
            MenuItemVm.MenuItem.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());

            if (!ModelState.IsValid)
            {
                MenuItemVm.SubCategory = await _db.SubCategory.Where(s => s.CategoryId == MenuItemVm.MenuItem.CategoryId).ToListAsync();
                return View(MenuItemVm);
            }

            // working on the image saving section

            var webRootPath = _webHostEnviroment.WebRootPath;
            var files = HttpContext.Request.Form.Files;

            var MenuItemFromDb = await _db.MenuItem.FindAsync(MenuItemVm.MenuItem.Id);

            

            if (files.Count() > 0)
            {
                //file has been uploaded

                var uploads = Path.Combine(webRootPath, "images");
                var extension_new = Path.GetExtension(files[0].FileName);

                //Delete the original file

                var imagePath = Path.Combine(webRootPath, MenuItemFromDb.Image.TrimStart('\\'));

                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }

                using (var filesStream = new FileStream(Path.Combine(uploads, MenuItemVm.MenuItem.Id + extension_new), FileMode.Create))
                {
                    files[0].CopyTo(filesStream);

                    MenuItemFromDb.Image = @"\images\" + MenuItemVm.MenuItem.Id + extension_new;

                }

                
            }

            MenuItemFromDb.Name = MenuItemVm.MenuItem.Name;
            MenuItemFromDb.Description = MenuItemVm.MenuItem.Description;
            MenuItemFromDb.Price = MenuItemVm.MenuItem.Price;
            MenuItemFromDb.Category = MenuItemVm.MenuItem.Category;
            MenuItemFromDb.SubCategory = MenuItemVm.MenuItem.SubCategory;
            MenuItemFromDb.Spicyness = MenuItemVm.MenuItem.Spicyness;

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));


        }

        public async Task <IActionResult> Details(int ?id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var MenuItemFromDb = await _db.MenuItem.Include(s => s.Category).Include(s => s.SubCategory).SingleOrDefaultAsync(s => s.Id == id);

            if (MenuItemFromDb == null)
            {
                return NotFound();
            }

            return View(MenuItemFromDb);
        }

        public async Task<IActionResult> Delete(int?id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var MenuItemFromDb = await _db.MenuItem.Include(s => s.Category).Include(s => s.SubCategory).SingleOrDefaultAsync(s => s.Id == id);

            if (MenuItemFromDb == null)
            {
                return NotFound();
            }

            return View(MenuItemFromDb);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeletePOST(int ? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            var MenuItemFromDb = await _db.MenuItem.Include(s => s.Category).Include(s => s.SubCategory).SingleOrDefaultAsync(s => s.Id == id);

            if (MenuItemFromDb == null)
            {
                return NotFound();
            }

            _db.MenuItem.Remove(MenuItemFromDb);

            // Removing The Image from the server

            var webRootPath = _webHostEnviroment.WebRootPath;
            var files = HttpContext.Request.Form.Files;

            if (files.Count() > 0)
            {
                var imagePath = Path.Combine(webRootPath, MenuItemFromDb.Image.TrimStart('\\'));

                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
