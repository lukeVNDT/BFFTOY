using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PagedList.Core;
using Tfood.Extension;
using Tfood.Models;

namespace Tfood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class RolesController : Controller
    {
        private readonly tfoodContext _context;
        public INotyfService _notifyService { get; }

        public RolesController(tfoodContext context, INotyfService notifyService)
        {
            _context = context;
            _notifyService = notifyService;
        }

        // GET: Admin/Roles
        public async Task<IActionResult> Index()
        {
            var AccountId = HttpContext.Session.GetString("AccountId");
            if (String.IsNullOrEmpty(AccountId))
            {
                AccountId = User.Identity.GetAccountID();
            }
            var account = await _context.Accounts
                .AsNoTracking()
                .Where(x => x.RoleId == 34)
                .Where(x => x.Id == Convert.ToInt32(AccountId))
                .SingleOrDefaultAsync();
            if (account != null)
            {
                var pageNumber = 1;
                var pageSize = 10;
                List<Role> listRole = new List<Role>();
                listRole = _context.Roles.AsNoTracking()
                   .OrderByDescending(x => x.Id)
                   .ToList();

                PagedList<Role> models = new PagedList<Role>(listRole.AsQueryable(), pageNumber, pageSize);
                ViewBag.CurrentPage = pageNumber;
                return View(models);
            }   
            else
            {
                return View("AccessDeniedView");
            }    
            
        }

        // GET: Admin/Roles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Roles == null)
            {
                return NotFound();
            }

            var role = await _context.Roles
                .FirstOrDefaultAsync(m => m.Id == id);
            if (role == null)
            {
                return NotFound();
            }

            return View(role);
        }

        // GET: Admin/Roles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Roles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description")] Role role)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(role);
                    await _context.SaveChangesAsync();
                    _notifyService.Success("Tạo quyền truy cập thành công");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _notifyService.Error("Tạo quyền truy cập thất bại, vui lòng kiểm tra lại.");
                    return RedirectToAction(nameof(Index));
                }
            }
            _notifyService.Error("Tạo quyền truy cập thất bại, vui lòng kiểm tra lại.");
            return RedirectToAction(nameof(Index));
        }


        // GET: Admin/Roles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Roles == null)
            {
                return NotFound();
            }

            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            return View(role);
        }

        // POST: Admin/Roles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] Role role)
        {
            if (id != role.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(role);
                    await _context.SaveChangesAsync();
                    _notifyService.Success("Chỉnh sửa quyền truy cập thành công");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    _notifyService.Error("Chỉnh sửa quyền truy cập thất bại, vui lòng kiểm tra lại.");
                    return RedirectToAction(nameof(Index));
                }
                
            }
            _notifyService.Error("Chỉnh sửa quyền truy cập thất bại, vui lòng kiểm tra lại.");
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Roles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Roles == null)
            {
                return NotFound();
            }

            var role = await _context.Roles
                .FirstOrDefaultAsync(m => m.Id == id);
            if (role == null)
            {
                return NotFound();
            }

            return View(role);
        }

        // POST: Admin/Roles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Roles == null)
            {
                return Problem("Entity set 'tfoodContext.Roles'  is null.");
            }
            var role = await _context.Roles.FindAsync(id);
            if (role != null)
            {
                _context.Roles.Remove(role);
            }
            
            await _context.SaveChangesAsync();
            _notifyService.Success("Xóa quyền truy cập thành công");
            //return RedirectToAction(nameof(Index));
            return NoContent();
        }

        private bool RoleExists(int id)
        {
          return _context.Roles.Any(e => e.Id == id);
        }
    }
}
