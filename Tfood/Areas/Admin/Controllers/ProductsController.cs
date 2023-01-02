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
    public class ProductsController : Controller
    {
        public INotyfService _notifyService { get; }
        private readonly tfoodContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public ProductsController(tfoodContext context, IWebHostEnvironment hostingEnvironment, INotyfService notifyService)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
            _notifyService = notifyService;
        }

        // GET: Admin/Products/Filter
        public IActionResult Filter(int cateid = 0)
        {
            var url = $"/Admin/Products?cateid={cateid}";
            if (cateid == 0)
            {
                url = "/Admin/Products";
            }
            return Json(new { status = "success", redirectUrl = url });
        }

        // GET: Admin/Products
        public async Task<IActionResult> Index(int page = 1, int cateid = 0)
        {
            var pageNumber = page;
            var pageSize = 10;
            List<Product> listProduct = new List<Product>();
            if (cateid != 0)
            {
                listProduct = _context.Products.AsNoTracking()
                    .Where(x => x.CatId == cateid)
                    .Include(x => x.Cat)
                    .OrderByDescending(x => x.Id)
                    .ToList();
            }   
            else
            {
                listProduct = _context.Products.AsNoTracking()
                .Include(x => x.Cat)
                .OrderByDescending(x => x.Id)
                .ToList();
            }    
            PagedList<Product> models = new PagedList<Product>(listProduct.AsQueryable(), pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            ViewBag.CurrentCateid = cateid;
            ViewData["Category"] = new SelectList(_context.Categories, "Id", "Name", cateid);
            return View(models);
        }

        // GET: Admin/Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Cat)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Admin/Products/Create
        public IActionResult Create()
        {
            ViewData["Cate"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // POST: Admin/Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,ShortDesc,Description,CatId,Price,Thumb,DateCreate,IsBestSeller,HomeFlag,Active,Tags,Title,Alias,MetaDesc,MetaKey,UnitInStock")] Product product, IFormFile? ProductThumb)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    product.Name = Utilities.ToTitleCase(product.Name);
                    if (ProductThumb != null)
                    {
                        string extension = Path.GetExtension(ProductThumb.FileName);
                        string image = Utilities.SEOUrl(product.Name) + extension;
                        product.Thumb = await Utilities.UploadFile(ProductThumb, @"products", image.ToLower());
                    }
                    if (ProductThumb == null) product.Thumb = "";
                    product.Alias = Utilities.SEOUrl(product.Name);
                    product.DateCreate = DateTime.Now;

                    _context.Add(product);
                    _notifyService.Success("Tạo sản phẩm thành công");
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", "Products");
                }
                catch (Exception ex)
                {
                    _notifyService.Error("Tạo sản phẩm thất bại, vui lòng kiểm tra lại!");
                    return RedirectToAction(nameof(Index));
                }
            }
            ViewData["CatId"] = new SelectList(_context.Categories, "Id", "Id", product.CatId);
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["Cate"] = new SelectList(_context.Categories, "Id", "Name", product.CatId);
            return View(product);
        }

        // POST: Admin/Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,ShortDesc,Description,CatId,Price,Thumb,DateCreate,IsBestSeller,HomeFlag,Active,Tags,Title,Alias,MetaDesc,MetaKey,UnitInStock")] Product product, IFormFile? ProductThumb)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    product.Name = Utilities.ToTitleCase(product.Name);
                    if (ProductThumb != null)
                    {
                        string extension = Path.GetExtension(ProductThumb.FileName);
                        string image = Utilities.SEOUrl(product.Name) + extension;
                        product.Thumb = await Utilities.UploadFile(ProductThumb, @"products", image.ToLower());
                        
                    }
                    if (string.IsNullOrEmpty(product.Thumb)) product.Thumb = "";
                    product.Alias = Utilities.SEOUrl(product.Name);
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                    _notifyService.Success("Cập nhật sản phẩm thành công");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    _notifyService.Error("Cập nhật sản phẩm thất bại. Vui lòng kiểm tra lại!");
                    return RedirectToAction(nameof(Index));
                }
                
                
            }
            _notifyService.Error("Cập nhật sản phẩm thất bại. Vui lòng kiểm tra lại!");
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Cat)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Admin/Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Products == null)
            {
                return Problem("Entity set 'tfoodContext.Products'  is null.");
            }
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
          return _context.Products.Any(e => e.Id == id);
        }
    }
}
