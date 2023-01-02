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
    public class PagesController : Controller
    {
        private readonly tfoodContext _context;
        public INotyfService _notifyService { get; }
        public PagesController(tfoodContext context, INotyfService notifyService)
        {
            _context = context;
            _notifyService = notifyService;
        }

        // GET: Admin/Pages
        public async Task<IActionResult> Index(int? page)
        {
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 10;
            var listPages = _context.Pages.AsNoTracking()
                .OrderByDescending(x => x.Id);
            PagedList<Page> models = new PagedList<Page>(listPages, pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            return View(models);
        }

        // GET: Admin/Pages/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Pages == null)
            {
                return NotFound();
            }

            var page = await _context.Pages
                .FirstOrDefaultAsync(m => m.Id == id);
            if (page == null)
            {
                return NotFound();
            }

            return View(page);
        }

        // GET: Admin/Pages/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Pages/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Contents,Thumb,Published,Title,MetaDesc,MetaKey,Alias,CreateDate,Sequence,Icon")] Page page, IFormFile? PagesThumb)
        {
            if (ModelState.IsValid)
            {
                page.Name = Utilities.ToTitleCase(page.Name);
                if (PagesThumb != null)
                {
                    string extension = Path.GetExtension(PagesThumb.FileName);
                    string image = Utilities.SEOUrl(page.Name) + extension;
                    page.Thumb = await Utilities.UploadFile(PagesThumb, @"pages", image.ToLower());
                }
                if (PagesThumb == null) page.Thumb = "";
                page.Alias = Utilities.SEOUrl(page.Name);
                page.CreateDate = DateTime.Now;

                _context.Add(page);
                await _context.SaveChangesAsync();
                _notifyService.Success("Tạo page thành công");
                return RedirectToAction(nameof(Index));
            }
            _notifyService.Error("Tạo page thất bại");
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Pages/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Pages == null)
            {
                return NotFound();
            }

            var page = await _context.Pages.FindAsync(id);
            if (page == null)
            {
                return NotFound();
            }
            return View(page);
        }

        // POST: Admin/Pages/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Contents,Thumb,Published,Title,MetaDesc,MetaKey,Alias,CreateDate,Sequence,Icon")] Page page, IFormFile? PagesThumb)
        {
            if (id != page.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    page.Name = Utilities.ToTitleCase(page.Name);
                    if (PagesThumb != null)
                    {
                        string extension = Path.GetExtension(PagesThumb.FileName);
                        string image = Utilities.SEOUrl(page.Name) + extension;
                        page.Thumb = await Utilities.UploadFile(PagesThumb, @"pages", image.ToLower());
                    }
                   
                    page.Alias = Utilities.SEOUrl(page.Name);
                    page.CreateDate = DateTime.Now;

                    _context.Update(page);
                    await _context.SaveChangesAsync();
                    _notifyService.Success("Cập nhật page thành công");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    _notifyService.Error("Cập nhật page thất bại, vui lòng kiểm tra lại");
                    return RedirectToAction(nameof(Index));
                }
            }
            _notifyService.Error("Cập nhật page thất bại");
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Pages/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Pages == null)
            {
                return NotFound();
            }

            var page = await _context.Pages
                .FirstOrDefaultAsync(m => m.Id == id);
            if (page == null)
            {
                return NotFound();
            }

            return View(page);
        }

        // POST: Admin/Pages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Pages == null)
            {
                return Problem("Entity set 'tfoodContext.Pages'  is null.");
            }
            var page = await _context.Pages.FindAsync(id);
            if (page != null)
            {
                _context.Pages.Remove(page);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PageExists(int id)
        {
          return _context.Pages.Any(e => e.Id == id);
        }
    }
}
