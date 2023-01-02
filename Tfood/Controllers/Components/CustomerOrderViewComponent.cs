using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PagedList.Core;
using Tfood.Models;
using Tfood.ViewModel;

namespace Tfood.Controllers.Components
{
    public class CustomerOrderViewComponent : ViewComponent
    {
        private readonly tfoodContext _context;

        public CustomerOrderViewComponent(tfoodContext context)
        {
            _context = context;
        }    
        public IViewComponentResult Invoke(int ID)
        {
            
            var order = _context.Orders
                .AsNoTracking()
                .Where(x => x.Id == ID)
                .ToList();

            var orderDetail = _context.OrderDetails
                    .Include(x => x.Product)
                    .AsNoTracking()
                    .Where(x => x.OrderId == ID)
                    .OrderBy(x => x.Id)
                    .ToList();

            ViewBag.Order = order;
            return View(orderDetail);
        }
    }
}
