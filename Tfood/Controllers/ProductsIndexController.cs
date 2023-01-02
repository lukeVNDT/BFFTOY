using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PagedList.Core;
using Tfood.Models;

namespace Tfood.Controllers
{
    public class ProductsIndexController : Controller
    {
        private readonly tfoodContext _context;

        public ProductsIndexController(tfoodContext context)
        {
            _context = context;
        }

        // GET: ProductsIndex
        public async Task<IActionResult> Index(int page = 1)
        {
            var pageNumber = page;
            var pageSize = 16;
            List<Product> listProduct = new List<Product>();
            listProduct = _context.Products.AsNoTracking()
               .Include(x => x.Cat)
               .OrderByDescending(x => x.Id)
               .ToList();

            PagedList<Product> models = new PagedList<Product>(listProduct.AsQueryable(), pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            return View(models);
        }

        // GET: ProductsIndex/Details/5
        [Route("/product-detail/{Alias}--{Id}.html", Name = "ProductDetail")]
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
            var customerid = HttpContext.Session.GetString("CustomerId");

            if (customerid != null)
            {
                var favorite = await _context.Favorites
                .AsNoTracking()
                .Where(x => x.ProductId == id)
                .Where(x => x.CustomerId == Convert.ToInt32(customerid))
                .ToListAsync();

                if (favorite != null) ViewBag.Favorite = favorite;
            }  
            else
            {
                List<Favorite> emptyList = new List<Favorite>();
                ViewBag.Favorite = emptyList;
            }    
            

            var relationalProduct = await _context.Products
                .AsNoTracking()
                .Include(p => p.Cat)
                .Where(p => p.Id != id && p.CatId == product.CatId && p.Active == true)
                .OrderByDescending(p => p.DateCreate)
                .Take(8)
                .ToListAsync();

            ViewBag.RelationalProduct = relationalProduct;

            return View(product);
        }

        // GET: ProductsIndex/Create
        public IActionResult Create()
        {
            ViewData["CatId"] = new SelectList(_context.Categories, "Id", "Id");
            return View();
        }

        // POST: ProductsIndex/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,ShortDesc,Description,CatId,Price,Discount,Thumb,DateCreate,IsBestSeller,HomeFlag,Active,Tags,Title,Alias,MetaDesc,MetaKey,UnitInStock")] Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CatId"] = new SelectList(_context.Categories, "Id", "Id", product.CatId);
            return View(product);
        }

        // GET: ProductsIndex/Edit/5
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
            ViewData["CatId"] = new SelectList(_context.Categories, "Id", "Id", product.CatId);
            return View(product);
        }

        // POST: ProductsIndex/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,ShortDesc,Description,CatId,Price,Discount,Thumb,DateCreate,IsBestSeller,HomeFlag,Active,Tags,Title,Alias,MetaDesc,MetaKey,UnitInStock")] Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CatId"] = new SelectList(_context.Categories, "Id", "Id", product.CatId);
            return View(product);
        }

        // GET: ProductsIndex/Delete/5
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

        // POST: ProductsIndex/Delete/5
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
