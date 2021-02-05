using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;

namespace Spice.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CouponController : Controller
    {
        private readonly ApplicationDbContext _db;
        public CouponController(ApplicationDbContext db)
        {
            _db = db;
        }

        [BindProperty]
        public Coupon coupon { get; set; }
        public async Task<IActionResult> Index()
        {
            return View(await _db.Coupon.ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost, ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePOST()
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var files = HttpContext.Request.Form.Files;
            

            if (files.Count() > 0)
            {
                byte[] p1 = null;
                using (var fs1 = files[0].OpenReadStream())
                {
                    using (var ms1 = new MemoryStream())
                    {
                        fs1.CopyTo(ms1);
                        p1 = ms1.ToArray();
                    }
                }
                coupon.Picture = p1;
            }

           

            _db.Coupon.Add(coupon);
            await _db.SaveChangesAsync();

           return RedirectToAction(nameof(Index));

        }

        public async Task<IActionResult> Edit(int ? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var CouponFromDb =await _db.Coupon.SingleOrDefaultAsync(s=> s.Id==id);

            if (CouponFromDb == null)
            {
                return NotFound();
            }

            return View(CouponFromDb);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit()
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            if (coupon == null)
            {
                return NotFound();
            }

            
            var files = HttpContext.Request.Form.Files;

            if (files.Count() > 0)
            {
                var imageFromDb = Encoding.Default.GetString(coupon.Picture);
                if (System.IO.File.Exists(imageFromDb))
                {
                    System.IO.File.Delete(imageFromDb);
                }


                byte[] p1 = null;
                using(var fs1 = files[0].OpenReadStream())
                {
                    using(var ms1 = new MemoryStream())
                    {
                        fs1.CopyTo(ms1);
                        p1 = ms1.ToArray();
                    }
                }

                coupon.Picture = p1;
               
            }

            _db.Coupon.Update(coupon);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Details(int ? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var CouponFromDb = _db.Coupon.FindAsync(id);
            if (CouponFromDb == null)
            {
                return NotFound();
            }

            return View(CouponFromDb);
        }
    }
}
