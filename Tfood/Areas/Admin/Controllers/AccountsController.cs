using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PagedList.Core;
using Tfood.Areas.Admin.ViewModel;
using Tfood.Extension;
using Tfood.Helpper;
using Tfood.Models;

namespace Tfood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class AccountsController : Controller
    {
        private readonly tfoodContext _context;
        public INotyfService _notifyService { get; }
        public AccountsController(tfoodContext context, INotyfService notifyService)
        {
            _context = context;
            _notifyService = notifyService;
        }

        public async Task<Account> ValidateEmail(string Email)
        {
            var account = await _context.Accounts.AsNoTracking().FirstOrDefaultAsync(
                x => x.Email.ToLower() == Email.ToLower());
            return account;
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("doi-mat-khau-quan-tri.html", Name = "ChangePasswordAdministrator")]
        public async Task<IActionResult> ChangePassword(AdministratorChangePasswordVM change)
        {
            var AccountId = HttpContext.Session.GetString("AccountId");
            if (AccountId == null)
            {
                AccountId = User.Identity.GetAccountID();
            }    
            if (ModelState.IsValid)
            {
                var account = await _context.Accounts
                    .FindAsync(Convert.ToInt32(AccountId));
                if (account == null) return RedirectToAction("LoginAdmin", "Accounts");
                var currentPass = (change.PasswordNow.Trim() + account.Salt.Trim()).ToMD5();
                if (currentPass != account.Password) return Json(new { status = "failed", data = "Mật khẩu hiện tại của bạn nhập vào không đúng!" });
                var pass = change.ConfirmPassword;
                if (pass != change.Password) return Json(new { status = "failed", data = "Xác nhận thất bại, mật khẩu không khớp!" });
                var newPass = (change.Password.Trim() + account.Salt.Trim()).ToMD5();
                account.Password = newPass;
                _context.Update(account);
                _context.SaveChangesAsync();
                return RedirectToAction("GetAdministratorProfile", "Accounts");
            }
            return Json(new { status = "failed", data = "Thay đổi mật khẩu không thành công" });
        }

        [AllowAnonymous]
        [Route("dang-nhap-quan-tri.html", Name = "AdminLogin")]
        public IActionResult LoginAdmin()
        {
            return PartialView("_AccountLoginPartialView");
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("dang-nhap-quan-tri.html", Name = "AdminLogin")]
        public async Task<IActionResult> LoginAdmin(AccountLoginVM model)
        {
            if (ModelState.IsValid)
            {
                var account = await _context.Accounts
                    .Include(x => x.Role)
                    .AsNoTracking().
                    FirstOrDefaultAsync(x => x.Email == model.UserName.Trim().ToLower());
                if (account == null) return Json(new { status = "failed", data = "Sai thông tin đăng nhập" });
                string pass = (model.Password + account.Salt.Trim()).ToMD5();
                if (account.Password != pass) return Json(new { status = "failed", data = "Sai thông tin đăng nhập" });


                // save Session Customer
                HttpContext.Session.SetString("AccountId", account.Id.ToString());
                HttpContext.Session.SetString("AccountName", account.Fullname);
                //identity
                var userClaims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, account.Fullname),
                        new Claim(ClaimTypes.Email, account.Email),
                        new Claim("AccountId", account.Id.ToString()),
                        new Claim("RoleId", account.RoleId.ToString()),
                        new Claim(ClaimTypes.Role, account.Role.Name)
                    };
                var grandmaIdentity = new ClaimsIdentity(userClaims, "User Identity");
                var userPrincipal = new ClaimsPrincipal(new[] { grandmaIdentity });
                await HttpContext.SignInAsync(userPrincipal);
                return Json(new { status = "success", data = "Đăng nhập thành công" });
            }
            else
            {
                return Json(new { status = "failed", data = "Sai thông tin đăng nhập" });
            }

        }

        // GET: Admin/Accounts/Filter
        public IActionResult Filter(int tranid = 0)
        {
            var url = $"/Admin/Accounts?tranid={tranid}";
            if (tranid == 0)
            {
                url = "/Admin/Accounts";
            }
            return Json(new { status = "success", redirectUrl = url });
        }

        // GET: Admin/Accounts
        [AllowAnonymous]
        public async Task<IActionResult> Index(int tranid = 0)
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
                List<Account> listAccount = new List<Account>();
                if (tranid != 0)
                {
                    listAccount = _context.Accounts.AsNoTracking()
                   .Include(x => x.Role)
                   .Where(x => x.Deactivate == tranid)
                   .OrderByDescending(x => x.Id)
                   .ToList();
                }
                else
                {
                    listAccount = _context.Accounts.AsNoTracking()
                   .Include(x => x.Role)
                   .OrderByDescending(x => x.Id)
                   .ToList();
                }

                PagedList<Account> models = new PagedList<Account>(listAccount.AsQueryable(), pageNumber, pageSize);
                ViewBag.CurrentPage = pageNumber;

                List<SelectListItem> listStatus = new List<SelectListItem>();
                listStatus.Add(new SelectListItem() { Text = "Kích hoạt", Value = "0" });
                listStatus.Add(new SelectListItem() { Text = "Khóa", Value = "1" });

                ViewData["ListStatus"] = new SelectList(listStatus.AsQueryable(), "Value", "Text", tranid);

                return View(models);
            }
            else
            {
                return View("AccessDeniedView");
            }

        }

        [HttpPost, ActionName("Ban")]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Ban(int accountid)
        {
            if (accountid == null)
            {
                _notifyService.Error("Chặn tài khoản thất bại");
                return RedirectToAction("Index", "Accounts");
            }    
            else
            {
                var userid = HttpContext.Session.GetString("AccountId");
                if (String.IsNullOrEmpty(userid))
                {
                    userid = User.Identity.GetAccountID();
                }
                if (accountid == Convert.ToInt32(userid))
                {
                    _notifyService.Error("Bạn không thể chặn chính tài khoản bạn đang đăng nhập");
                    return RedirectToAction("Index", "Accounts");
                }
                else 
                {
                    var account = await _context.Accounts
                                    .Where(x => x.Id == accountid)
                                    .FirstOrDefaultAsync();
                    if (account == null)
                    {
                        _notifyService.Error("Chặn tài khoản thất bại");
                        return RedirectToAction("Index", "Accounts");
                    }
            
                    account.Deactivate = 1;
                    _context.Update(account);
                    _context.SaveChangesAsync();
                    HttpContext.Session.Remove("AccountId");
                    _notifyService.Success("Chặn tài khoản thành công");
                    return RedirectToAction("Index", "Accounts");
                }
            }    
        }

        [HttpPost, ActionName("Unlock")]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Unlock(int accountid)
        {
            if (accountid == null)
            {
                _notifyService.Error("Mở khóa tài khoản thất bại");
                return RedirectToAction("Index", "Accounts");
            }
            else
            {
                var account = await _context.Accounts
                                    .Where(x => x.Id == accountid)
                                    .FirstOrDefaultAsync();
                if (account == null)
                {
                    _notifyService.Error("Mở khóa tài khoản thất bại");
                    return RedirectToAction("Index", "Accounts");
                }

                account.Deactivate = 0;
                _context.Update(account);
                _context.SaveChangesAsync();
                _notifyService.Success("Mở khóa tài khoản thành công");
                return RedirectToAction("Index", "Accounts");
            }
        }

        // GET: Admin/Accounts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Accounts == null)
            {
                return NotFound();
            }

            var account = await _context.Accounts
                .Include(a => a.Role)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (account == null)
            {
                return NotFound();
            }

            return View(account);
        }

        [AllowAnonymous]
        [Route("tao-tai-khoan.html")]
        // GET: Admin/Accounts/Create
        public IActionResult Create()
        {
            ViewData["RoleId"] = new SelectList(_context.Roles, "Id", "Name");
            return View();
        }

        [AllowAnonymous]
        [Route("tao-tai-khoan.html")]
        // POST: Admin/Accounts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Email,Password,Phone,Fullname,RoleId")] Account account)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var isExistEmail = await ValidateEmail(account.Email);
                    if (isExistEmail != null) return Json(new { status = "failed", data = "Email: " + account.Email + " này đã được sử dụng" });


                    string salt = Utilities.GetRandomKey();
                    Account acc = new Account
                    {
                        Fullname = account.Fullname,
                        Email = account.Email.Trim().ToLower(),
                        Password = (account.Password + salt.Trim()).ToMD5(),
                        RoleId = account.RoleId,
                        Salt = salt,
                        CreateDate = DateTime.Now
                    };

                    _context.Add(acc);
                    await _context.SaveChangesAsync();
                    _notifyService.Success("Tạo tài khoản thành công");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _notifyService.Error("Tạo tài khoản thất bại, vui lòng kiểm tra lại!");
                    return RedirectToAction(nameof(Index));
                }
            }
            ViewData["RoleId"] = new SelectList(_context.Roles, "Id", "Id", account.RoleId);
            _notifyService.Error("Tạo tài khoản thất bại, vui lòng kiểm tra lại!");
            return RedirectToAction(nameof(Index));
        }


        [AllowAnonymous]
        // GET: Admin/Accounts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Accounts == null)
            {
                return NotFound();
            }

            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
            {
                return NotFound();
            }
            ViewData["RoleId"] = new SelectList(_context.Roles, "Id", "Name", account.RoleId);
            return View(account);
        }

        // POST: Admin/Accounts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Email,Password,Phone,Fullname,RoleId,CreateDate,Salt,Deactivate")] Account account)
        {
            if (id != account.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(account);
                    await _context.SaveChangesAsync();
                    _notifyService.Success("Cập nhật tài khoản thành công");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _notifyService.Error("Cập nhật tài khoản thất bại, vui lòng kiểm tra lại!");
                    return RedirectToAction(nameof(Index));
                }
                
            }
            ViewData["RoleId"] = new SelectList(_context.Roles, "Id", "Id", account.RoleId);
            _notifyService.Error("Cập nhật tài khoản thất bại, vui lòng kiểm tra lại!");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(int userid, [Bind("Id,Email,Password,Phone,Fullname,RoleId,CreateDate,Salt,Deactivate")] Account account)
        {
            if (userid != account.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(account);
                    await _context.SaveChangesAsync();
                    _notifyService.Success("Cập nhật tài khoản thành công");
                    HttpContext.Session.SetString("AccountName", account.Fullname);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AccountExists(account.Id))
                    {
                        _notifyService.Error("Cập nhật tài khoản thất bại");
                        return NotFound();
                    }
                    else
                    {
                        _notifyService.Error("Cập nhật tài khoản thất bại");
                        throw;
                    }
                }
                return RedirectToAction("GetAdministratorProfile", "Accounts");
            }
            _notifyService.Error("Cập nhật tài khoản thất bại");
            return View(account);
        }

        // GET: Admin/Accounts/Delete/5
        [AllowAnonymous]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Accounts == null)
            {
                return NotFound();
            }

            var account = await _context.Accounts
                .Include(a => a.Role)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (account == null)
            {
                return NotFound();
            }

            return View(account);
        }

        // POST: Admin/Accounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Accounts == null)
            {
                return Problem("Entity set 'tfoodContext.Accounts'  is null.");
            }
            var account = await _context.Accounts.FindAsync(id);
            if (account != null)
            {
                _context.Accounts.Remove(account);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AccountExists(int id)
        {
          return _context.Accounts.Any(e => e.Id == id);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("trang-ca-nhan-quan-tri.html")]
        public async Task<IActionResult> GetAdministratorProfile()
        {
            var AdminId = HttpContext.Session.GetString("AccountId");
            if (AdminId == null)
            {
                AdminId = User.Identity.GetAccountID();
            }    
            if (AdminId != null)
            {
                var profile = await _context.Accounts
                    .Include(x => x.Role)
                    .AsNoTracking()
                    .Where(x => x.Id == Convert.ToInt32(AdminId))
                    .SingleOrDefaultAsync();
                if (profile != null)
                {
                    return View("ProfileAdministratorView", profile);
                }
                return StatusCode(404);
            }
            return StatusCode(404);
        }


        [HttpGet]
        [AllowAnonymous]
        [Route("dang-xuat-quan-tri.html")]
        public IActionResult LogoutAdministrator()
        {
            HttpContext.SignOutAsync();
            HttpContext.Session.Remove("AccountId");
            return RedirectToAction("Index", "Home");
        }
        
    }
}
