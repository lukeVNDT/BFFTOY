using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tfood.Models;

namespace Tfood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class HomeController : Controller
    {

        private readonly tfoodContext _context;
        public HomeController(tfoodContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            // get total product
            var TotalProduct = _context.Products
                .ToList();

            // get all customer 
            var TotalCustomer = _context.Customers
                .ToList();

            // get revenue in 30 days
            var TotalRevenue = _context.Orders
                .Where(x => x.Deleted == false)
                .Where(x => x.OrderDate.Value.Month == DateTime.Now.Month)
                .Where(x => x.TransactionStatusId == 8)
                .ToList();

            // get total order in 30 days
            var TotalOrder = _context.Orders
                .Where(x => x.Deleted == false)
                .ToList();

            ViewBag.TotalProduct = TotalProduct;
            ViewBag.TotalCustomer = TotalCustomer;
            ViewBag.TotalRevenue = TotalRevenue;
            ViewBag.TotalOrder = TotalOrder;

            return View();
        }
    }
}
