using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PagedList.Core;
using Tfood.Models;

namespace Tfood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly tfoodContext _context;

        public OrdersController(tfoodContext context)
        {
            _context = context;
        }

        // GET: Admin/Orders/Filter
        public IActionResult Filter(int tranid = 0)
        {
            var url = $"/Admin/Orders?tranid={tranid}";
            if (tranid == 0)
            {
                url = "/Admin/Orders";
            }
            return Json(new { status = "success", redirectUrl = url });
        }

        // GET: Admin/Orders
        public async Task<IActionResult> Index(int? page, int tranid = 0)
        {
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 10;
            List<Order> order = new List<Order>();
            if (tranid != 0)
            {
                order = await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.TransactionStatus)
                    .AsNoTracking()
                    .Where(x => x.Deleted == false)
                    .Where(x => x.TransactionStatusId == tranid)
                    .ToListAsync();
            }
            else
            {
                order = await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.TransactionStatus)
                    .AsNoTracking()
                    .Where(x => x.Deleted == false)
                    .ToListAsync();
            }
            PagedList<Order> models = new PagedList<Order>(order.AsQueryable(), pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            ViewData["TransactionStatus"] = new SelectList(_context.TransactionStatuses, "Id", "Description", tranid);
            return View(models);
        }

        // GET: Admin/Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Orders == null)
            {
                return NotFound();
            }

            var order = _context.Orders
                .AsNoTracking()
                .Include(x => x.TransactionStatus)
                .Where(x => x.Id == id)
                .ToList();

            var orderDetail = _context.OrderDetails
                    .Include(x => x.Product)
                    .AsNoTracking()
                    .Where(x => x.OrderId == id)
                    .OrderBy(x => x.Id)
                    .ToList();


            if (order == null)
            {
                return NotFound();
            }

            ViewBag.OrderDetail = orderDetail;
            return View(order);
        }

        // GET: Admin/Orders/Create
        public IActionResult Create()
        {
            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Id");
            ViewData["TransactionStatusId"] = new SelectList(_context.TransactionStatuses, "Id", "Id");
            return View();
        }

        // POST: Admin/Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CustomerId,OrderDate,ShipDate,TransactionStatusId,Deleted,Paid,PaymentDate,PaymentId,Note,FullName,Phone,Email,Address,Total,OrderMethod")] Order order)
        {
            if (ModelState.IsValid)
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Id", order.CustomerId);
            ViewData["TransactionStatusId"] = new SelectList(_context.TransactionStatuses, "Id", "Id", order.TransactionStatusId);
            return View(order);
        }

        // GET: Admin/Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Orders == null)
            {
                return NotFound();
            }


            var order = await _context.Orders
                .AsNoTracking()
                .Include(x => x.TransactionStatus)
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();

            var orderDetail = await _context.OrderDetails
                    .Include(x => x.Product)
                    .AsNoTracking()
                    .Where(x => x.OrderId == id)
                    .OrderBy(x => x.Id)
                    .ToListAsync();
            if (order == null)
            {
                return NotFound();
            }

            ViewData["TransactionStatusId"] = new SelectList(_context.TransactionStatuses, "Id", "Description", order.TransactionStatusId);
            List<Order> orders = new List<Order>();
            orders.Add(order);
            ViewBag.OrderDetail = orderDetail;


            return View(orders);
        }

        // POST: Admin/Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("thay-doi-trang-thai-don.html/{ID:int}", Name = "ChangeOrderStatus")]
        public async Task<IActionResult> ChangeStatus(int id, [Bind("Id,CustomerId,OrderDate,ShipDate,TransactionStatusId,Deleted,Paid,PaymentDate,PaymentId,Note,FullName,Phone,Email,Address,Total,OrderMethod")] Order order)
        {
            if (id != order.Id)
            {
                return NotFound();
            }


            var Order = await _context.Orders
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
            if (Order != null)
            {
                Order.Paid = order.Paid;
                Order.TransactionStatusId = order.TransactionStatusId;
                if (Order.Paid == true)
                {
                    Order.PaymentDate = DateTime.Now;
                }
                if (Order.TransactionStatusId == 7) Order.ShipDate = DateTime.Now;
                _context.Update(Order);
                await _context.SaveChangesAsync();
                return Json(new
                {
                    status = "success",
                    data = "Cập nhật trạng thái đơn hàng thành công"
                ,
                    returnUrl = "/Admin/Orders"
                });
            }
            


            return Json(new { status = "failed", data = "Cập nhật trạng thái đơn hàng thất bại" });

        }

        // GET: Admin/Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Orders == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.TransactionStatus)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Admin/Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Orders == null)
            {
                return Problem("Entity set 'tfoodContext.Orders'  is null.");
            }
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                order.Deleted = true;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }
    }
}
