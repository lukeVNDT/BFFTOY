using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tfood.Extension;
using Tfood.Models;
using Tfood.ViewModel;

namespace Tfood.Controllers
{
    public class CartController : Controller
    {
        private readonly tfoodContext _context;
        public CartController(tfoodContext context)
        {
            _context = context;
        }

        public List<CartVM> Cart
        {
            get
            {
                var cart = HttpContext.Session.Get<List<CartVM>>("Cart");
                if (cart == default(List<CartVM>))
                {
                    cart = new List<CartVM>();
                }
                return cart;
            }    
        }

        [HttpPost]
        [Route("Them-vao-gio.html", Name ="AddToCart")]
        public IActionResult AddToCart(int productID, int? amount)
        {
            List<CartVM> cart = Cart;

            try
            {
                //Them san pham vao gio hang
                CartVM item = cart.SingleOrDefault(p => p.product.Id == productID);
                if (item != null) // da co => cap nhat so luong
                {
                    item.amount = item.amount + amount.Value;
                    //luu lai session
                    HttpContext.Session.Set<List<CartVM>>("Cart", cart);
                }
                else
                {
                    Product product = _context.Products.SingleOrDefault(p => p.Id == productID);
                    item = new CartVM
                    {
                        amount = amount.HasValue ? amount.Value : 1,
                        product = product
                    };
                    cart.Add(item);//Them vao gio
                }

                //Luu lai Session
                HttpContext.Session.Set<List<CartVM>>("Cart", cart);
                return Json(new { status = "success", data = "Thêm sản phẩm vào giỏ thành công" });
            }
            catch
            {
                return Json(new { status = "failed", data = "Thêm sản phẩm vào giỏ thất bại" });
            }
        }
        [HttpPost]
        [Route("cap-nhat-gio-hang.html", Name ="UpdateCart")]
        public IActionResult UpdateCart(int productID, int? amount, string type)
        {
            //Lay gio hang ra de xu ly
            var cart = HttpContext.Session.Get<List<CartVM>>("Cart");
            try
            {
                if (cart != null)
                {
                    CartVM item = cart.SingleOrDefault(p => p.product.Id == productID);
                    if (item != null) // da co -> cap nhat so luong
                    {
                        if (type == "Increase")
                        {
                            item.amount = item.amount + amount.Value;
                        }    
                        else
                        {
                            item.amount = item.amount - amount.Value;
                        }    
                    }
                    //Luu lai session
                    HttpContext.Session.Set<List<CartVM>>("Cart", cart);
                }
                return Json(new { status = "success", data = "Cập nhật sản phẩm thành công" });
            }
            catch
            {
                return Json(new { status = "failed", data = "Cập nhật sản phẩm thất bại" });
            }
        }


        [HttpPost]
        [Route("cap-nhat-gio-hang-nhap-vao.html", Name = "UpdateCartInput")]
        public IActionResult UpdateCartInput(int productID, int? amount)
        {
            //Lay gio hang ra de xu ly
            var cart = HttpContext.Session.Get<List<CartVM>>("Cart");
            try
            {
                if (cart != null)
                {
                    CartVM item = cart.SingleOrDefault(p => p.product.Id == productID);
                    if (item != null) // da co -> cap nhat so luong
                    {
                        item.amount = amount.Value;
                    }
                    //Luu lai session
                    HttpContext.Session.Set<List<CartVM>>("Cart", cart);
                }
                return Json(new { status = "success", data = "Cập nhật sản phẩm thành công" });
            }
            catch
            {
                return Json(new { status = "failed", data = "Cập nhật sản phẩm thất bại" });
            }
        }


        [HttpPost]
        [Route("xoa-gio-hang.html", Name ="RemoveCart")]
        public ActionResult Remove(int productID)
        {

            try
            {
                List<CartVM> gioHang = Cart;
                CartVM item = gioHang.SingleOrDefault(p => p.product.Id == productID);
                if (item != null)
                {
                    gioHang.Remove(item);
                }
                //luu lai session
                HttpContext.Session.Set<List<CartVM>>("Cart", gioHang);
                return Json(new { status = "success", data = "Xóa sản phẩm khỏi giỏ thành công" });
            }
            catch
            {
                return Json(new { status = "failed", data = "Xóa sản phẩm khỏi giỏ thất bại" });
            }
        }
        [Route("gio-hang.html", Name = "Cart")]
        public IActionResult Index()
        {
            return View(Cart);
        }
    }
}
