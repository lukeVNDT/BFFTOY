using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PagedList.Core;
using Tfood.Helpper;
using Tfood.Models;

namespace Tfood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class CategoriesController : Controller
    {
        private readonly tfoodContext _context;
        public INotyfService _notifyService { get; }
        public CategoriesController(tfoodContext context, INotyfService notifyService)
        {
            _context = context;
            _notifyService = notifyService;
        }

        // GET: Admin/Categories
        public async Task<IActionResult> Index()
        {
            var pageNumber = 1;
            var pageSize = 10;
            List<Category> listCategory = new List<Category>();
            listCategory = _context.Categories.AsNoTracking()
               .OrderByDescending(x => x.Id)
               .ToList();

            PagedList<Category> models = new PagedList<Category>(listCategory.AsQueryable(), pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            return View(models);
        }

        // GET: Admin/Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: Admin/Categories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,ParentId,Thumb")] Category category, IFormFile? CategoriesThumb)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    category.Name = Utilities.ToTitleCase(category.Name);
                    if (CategoriesThumb != null)
                    {
                        string extension = Path.GetExtension(CategoriesThumb.FileName);
                        string image = Utilities.SEOUrl(category.Name) + extension;
                        category.Thumb = await Utilities.UploadFile(CategoriesThumb, @"categories", image.ToLower());
                    }
                    if (CategoriesThumb == null) category.Thumb = "";

                    _context.Add(category);
                    await _context.SaveChangesAsync();
                    _notifyService.Success("Tạo danh mục thành công");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _notifyService.Error("Tạo danh mục thất bại. Vui lòng kiểm tra lại!");
                    return RedirectToAction(nameof(Index));
                }
            }
            else
            {
                _notifyService.Error("Tạo danh mục thất bại. Vui lòng kiểm tra lại!");
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Admin/Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Admin/Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,ParentId,Thumb")] Category category, IFormFile? CategoriesThumb)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    category.Name = Utilities.ToTitleCase(category.Name);
                    if (CategoriesThumb != null)
                    {
                        string extension = Path.GetExtension(CategoriesThumb.FileName);
                        string image = Utilities.SEOUrl(category.Name) + extension;
                        category.Thumb = await Utilities.UploadFile(CategoriesThumb, @"categories", image.ToLower());
                    }
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                    _notifyService.Success("Cập nhật danh mục thành công");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _notifyService.Error("Cập nhật danh mục thất bại. Vui lòng kiểm tra lại!");
                    return RedirectToAction(nameof(Index));
                }

            }
            else
            {
                _notifyService.Error("Cập nhật danh mục thất bại. Vui lòng kiểm tra lại!");
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Admin/Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Categories == null)
            {
                return Problem("Entity set 'tfoodContext.Categories'  is null.");
            }
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}
