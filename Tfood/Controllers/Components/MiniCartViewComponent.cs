using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tfood.Extension;
using Tfood.ViewModel;

namespace Tfood.Controllers.Components
{
    public class MiniCartViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var cart = HttpContext.Session.Get<List<CartVM>>("Cart");
            return View(cart);
        }
    }
}
