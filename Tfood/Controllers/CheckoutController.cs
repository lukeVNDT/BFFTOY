using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tfood.Extension;
using Tfood.Models;
using Tfood.ViewModel;

namespace Tfood.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly tfoodContext _context;

        public CheckoutController(tfoodContext context)
        {
            _context = context;
        }

        [Route("thanh-toan.html", Name ="Checkout")]
        public IActionResult Index()
        {
            var cart = HttpContext.Session.Get<List<CartVM>>("Cart");
            ViewBag.Cart = cart;
            return View();
        }

        [HttpPost]
        [Route("dat-hang.html", Name ="Order")]
        public async Task<IActionResult> Order(CheckoutVM model)
        {
            var cart = HttpContext.Session.Get<List<CartVM>>("Cart");
            if (ModelState.IsValid)
            {
                if (cart != null)
                {
                    var customerId = HttpContext.Session.GetString("CustomerId");
                    if (!string.IsNullOrEmpty(customerId))
                    {
                        Order order = new Order();
                        order.CustomerId = model.CustomerId;
                        order.FullName = model.FullName;
                        order.Phone = model.Phone;
                        order.Email = model.Email;
                        order.Address = model.Address;
                        order.Total = cart.Sum(x => x.total);
                        order.Note = model.Note;
                        order.Deleted = false;
                        order.Paid = false;
                        order.OrderMethod = model.OrderMethod;
                        order.TransactionStatusId = 5;
                        order.OrderDate = DateTime.Now;

                        _context.Add(order);
                        await _context.SaveChangesAsync();

                        foreach (var item in cart)
                        {
                            OrderDetail detail = new OrderDetail();
                            detail.OrderId = order.Id;
                            detail.ProductId = item.product.Id;
                            detail.Quantity = item.amount;
                            detail.Price = item.product.Price;
                            detail.CreateDate = DateTime.Now;
                            _context.Add(detail);
                            _context.SaveChanges();
                        }
                        HttpContext.Session.Remove("Cart");
                        return Json(new { status = "success", data = "Đặt hàng thành công", returnUrl = "/cam-on.html" });

                    }
                    return Json(new { status = "failed", data = "Đặt hàng thất bại, vui lòng kiểm tra lại" });
                }    
            }
            return Json(new { status = "failed", data = "Đặt hàng thất bại, vui lòng điền đầy đủ thông tin mua hàng" }); 
        }


        [Route("lay-thong-tin.html", Name = "GetCustomerInformation")]
        public async Task<IActionResult> GetCustomerInformation()
        {
            var customerId = HttpContext.Session.GetString("CustomerId");
            if (customerId != null)
            {
                var customer = await _context.Customers.FirstOrDefaultAsync(x => x.Id == Convert.ToInt32(customerId));
                if (customer != null)
                {
                    return Json(new { status = "success", msg = "Lấy thông tin thành công",
                        fullName = customer.Fullname, email = customer.Email, phone = customer.Phone,
                        address = customer.Address
                        });
                }
                else
                {
                    return Json(new { status = "failed", msg = "Lấy thông tin thất bại" });
                }
            }
            return View();
        }

        [Route("cam-on.html", Name ="Thank")]
        public IActionResult Thank()
        {
            return View("OrderSuccess");
        }    
    }
}
