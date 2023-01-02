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
    public class PostCategoriesController : Controller
    {
        private readonly tfoodContext _context;
        public INotyfService _notifyService { get; }
        public PostCategoriesController(tfoodContext context, INotyfService notifyService)
        {
            _context = context;
            _notifyService = notifyService;
        }

        // GET: Admin/PostCategories
        public async Task<IActionResult> Index()
        {
            var pageNumber = 1;
            var pageSize = 10;
            List<PostCategory> listPostCategory = new List<PostCategory>();
            listPostCategory = _context.PostCategories.AsNoTracking()
               .OrderByDescending(x => x.Id)
               .ToList();

            PagedList<PostCategory> models = new PagedList<PostCategory>(listPostCategory.AsQueryable(), pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            return View(models);
        }

        // GET: Admin/PostCategories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.PostCategories == null)
            {
                return NotFound();
            }

            var postCategory = await _context.PostCategories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (postCategory == null)
            {
                return NotFound();
            }

            return View(postCategory);
        }

        // GET: Admin/PostCategories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/PostCategories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Thumb")] PostCategory postCategory, IFormFile? PostCategoriesThumb)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    postCategory.Name = Utilities.ToTitleCase(postCategory.Name);
                    if (PostCategoriesThumb != null)
                    {
                        string extension = Path.GetExtension(PostCategoriesThumb.FileName);
                        string image = Utilities.SEOUrl(postCategory.Name) + extension;
                        postCategory.Thumb = await Utilities.UploadFile(PostCategoriesThumb, @"postcategories", image.ToLower());
                    }
                    if (PostCategoriesThumb == null) postCategory.Thumb = "";

                    _context.Add(postCategory);
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

        // GET: Admin/PostCategories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.PostCategories == null)
            {
                return NotFound();
            }

            var postCategory = await _context.PostCategories.FindAsync(id);
            if (postCategory == null)
            {
                return NotFound();
            }
            return View(postCategory);
        }

        // POST: Admin/PostCategories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Thumb")] PostCategory postCategory, IFormFile? PostCategoriesThumb)
        {
            if (id != postCategory.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    postCategory.Name = Utilities.ToTitleCase(postCategory.Name);
                    if (PostCategoriesThumb != null)
                    {
                        string extension = Path.GetExtension(PostCategoriesThumb.FileName);
                        string image = Utilities.SEOUrl(postCategory.Name) + extension;
                        postCategory.Thumb = await Utilities.UploadFile(PostCategoriesThumb, @"postcategories", image.ToLower());
                    }

                    _context.Update(postCategory);
                    await _context.SaveChangesAsync();
                    _notifyService.Success("Cập nhật danh mục thành công");
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

        // GET: Admin/PostCategories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.PostCategories == null)
            {
                return NotFound();
            }

            var postCategory = await _context.PostCategories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (postCategory == null)
            {
                return NotFound();
            }

            return View(postCategory);
        }

        // POST: Admin/PostCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.PostCategories == null)
            {
                return Problem("Entity set 'tfoodContext.PostCategories'  is null.");
            }
            var postCategory = await _context.PostCategories.FindAsync(id);
            if (postCategory != null)
            {
                _context.PostCategories.Remove(postCategory);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PostCategoryExists(int id)
        {
          return _context.PostCategories.Any(e => e.Id == id);
        }
    }
}
