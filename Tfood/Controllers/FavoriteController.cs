using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tfood.Models;

namespace Tfood.Controllers
{
    [Authorize]
    public class FavoriteController : Controller
    {
        private readonly tfoodContext _context;

        public FavoriteController(tfoodContext context)
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> AddFavorite(int product_id)
        {
            var customerid = HttpContext.Session.GetString("CustomerId");
            if (string.IsNullOrEmpty(customerid)) return Json(new { status = "failed", data = "Bạn cần đăng nhập để yêu thích sản phẩm" });
            var favorite = await _context.Favorites
                .Where(x => x.CustomerId == Convert.ToInt32(customerid))
                .Where(x => x.ProductId == product_id)
                .FirstOrDefaultAsync();
            if (favorite != null)
            {
                _context.Favorites.Remove(favorite);
                await _context.SaveChangesAsync();
                return Json(new { status = "success-remove", data = "Đã bỏ yêu thích sản phẩm",
                icon = "<i class='fa fa-heart-o' aria-hidden='true'></i>"
                });
            }
            else if (favorite == null)
            {
                Favorite model = new Favorite();
                model.CustomerId = Convert.ToInt32(customerid);
                model.ProductId = product_id;
                _context.Add(model);
                await _context.SaveChangesAsync();
                return Json(new { status = "success", data = "Thêm sản phẩm yêu thích thành công" });
            }  
            return NoContent();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> RemoveFavorite(string productID, int customerID)
        {
            var favorite = await _context.Favorites
                .Where(x => x.CustomerId == Convert.ToInt32(customerID))
                .Where(x => x.ProductId == Convert.ToInt32(productID))
                .FirstOrDefaultAsync();
            if (favorite != null)
            {
                _context.Favorites.Remove(favorite);
                await _context.SaveChangesAsync();
                return Json(new { status = "success", data = "Xóa sản phẩm yêu thích thành công" });
            }
            else
            {
                return Json(new { status = "failed", data = "Xóa sản phẩm yêu thích thất bại" });
            }
        }
    }
}
