using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tfood.Models;
using Tfood.ViewModel;

namespace Tfood.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly tfoodContext _context;

        public HomeController(ILogger<HomeController> logger, tfoodContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            HomeVM model = new HomeVM();
            List<ProductHomeVM> listProductViews = new List<ProductHomeVM>();


            // Take list of product first
            var listProduct = _context.Products
                .AsNoTracking()
                .Where(x => x.Active == true && x.HomeFlag == true)
                .OrderByDescending(x => x.DateCreate)
                .ToList();

            // Take list of category
            var listCategory = _context.Categories
                .AsNoTracking()
                .OrderByDescending(x => x.Id)
                .ToList();

            foreach (var item in listCategory)
            {
                ProductHomeVM productHome = new ProductHomeVM();
                // Take three newest post
                var listPost = _context.Posts
                    .AsNoTracking()
                    .Where(x => x.Published == true && x.IsNewFeed == true)
                    .OrderByDescending(x => x.CreateDate)
                    .Take(3)
                    .ToList();

                productHome.Category = item;
                productHome.Products = listProduct.Where(x => x.CatId == item.Id).ToList();
                listProductViews.Add(productHome);

                model.Products = listProductViews;
                model.Posts = listPost;
                ViewBag.AllProduct = listProduct;
            }
            return View(model);


        }

        [HttpPost]
        public async Task<IActionResult> RecommendedSearch(string query)
        {

            var result = await _context.Products
                .AsNoTracking()
                .Where(x => EF.Functions.Like(x.Name, "%" + query + "%"))
                .ToListAsync();
            return PartialView("_RecommendSearchPartialView", result);

        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}