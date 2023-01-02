using Microsoft.AspNetCore.Mvc;

namespace Tfood.Controllers
{
    public class AjaxController : Controller
    {
        [Route("load-cart-ajax.html", Name ="LoadCart")]
        public IActionResult MiniCart()
        {
            return ViewComponent("MiniCart");
        }

        [Route("count-item-cart.html", Name ="CountItem")]
        public IActionResult CountItemCart()
        {
            return ViewComponent("CountItemCart");
        }
        [Route("load-cart-component.html", Name ="LoadCartComponent")]
        public IActionResult CartTable()
        {
            return ViewComponent("CartTable");
        }

        [HttpGet]
        [Route("load-order-list-customer.html/{ID:int}", Name = "LoadCustomerOrder")]
        public IActionResult CustomerOrder(int ID)
        {
            return ViewComponent("CustomerOrder", new {ID});
        }

        [HttpGet]
        [Route("load-favorite-index.html", Name = "LoadFavoriteIndex")]
        public IActionResult FavoriteIndex()
        {
            return ViewComponent("FavoriteIndex");
        }

        [Route("count-item-favorite.html", Name = "CountItemFavorite")]
        public IActionResult CountItemFavorite()
        {
            return ViewComponent("CountFavorite");
        }
    }
}
