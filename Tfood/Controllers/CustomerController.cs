using Microsoft.AspNetCore.Mvc;
using Tfood.Models;
using Tfood.ViewModel;
using System.Threading.Tasks;
using Tfood.Helpper;
using Tfood.Extension;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using PagedList.Core;
using AspNetCoreHero.ToastNotification.Abstractions;

namespace Tfood.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        private readonly tfoodContext _context;
        public INotyfService _notifyService { get; }
        public CustomerController(tfoodContext context, INotyfService notifyService)
        {
            _context = context;
            _notifyService = notifyService;
        }

        public async Task<Customer> ValidateEmail(string Email)
        {
            var customer = await _context.Customers.AsNoTracking().FirstOrDefaultAsync(
                x => x.Email.ToLower() == Email.ToLower());
            return customer;
        }

        public async Task<Customer> ValidatePhone(string phone)
        {
            var customer = await _context.Customers.AsNoTracking().FirstOrDefaultAsync(
                x => x.Phone.ToLower() == phone.ToLower());
            return customer;
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("dang-nhap.html", Name = "Login")]
        public IActionResult CustomerLogin()
        {
            return PartialView("_CustomerLoginPartialView");
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("dang-nhap.html", Name = "Login")]
        public async Task<IActionResult> CustomerLogin(LoginVM login)
        {
            if (ModelState.IsValid)
            {

                var customer = await _context.Customers.AsNoTracking().FirstOrDefaultAsync(x => x.Email == login.Email.Trim().ToLower());
                if (customer == null) return Json(new { status = "failed", data = "Sai thông tin đăng nhập" });
                string pass = (login.Password + customer.Salt.Trim()).ToMD5();
                if (customer.Password != pass) return Json(new { status = "failed", data = "Sai thông tin đăng nhập" });


                // save Session Customer
                HttpContext.Session.SetString("CustomerId", customer.Id.ToString());
                HttpContext.Session.SetString("CustomerName", customer.Fullname);
                HttpContext.Session.SetString("CustomerAvatar", customer.Avatar);
                // Identity
                var Claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, customer.Fullname),
                        new Claim("CustomerId", customer.Id.ToString())
                    };
                ClaimsIdentity claimsIdentity = new ClaimsIdentity(Claims, "Login");
                ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                await HttpContext.SignInAsync(claimsPrincipal);
                return Json(new { status = "success", data = "Đăng nhập thành công" });
            }
            return StatusCode(201);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("dang-ky.html", Name = "Register")]
        public IActionResult CustomerRegister()
        {
            return PartialView("_CustomerRegisterPartialView");
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("dang-ky.html", Name = "Register")]
        public async Task<IActionResult> CustomerRegister(RegisterVM register)
        {
            if (ModelState.IsValid)
            {
                var isExistEmail = await ValidateEmail(register.Email);
                if (isExistEmail != null) return Json(new { status = "failed", data = "Email: " + register.Email + " này đã được sử dụng" });

                var isExistPhone = await ValidatePhone(register.Phone);
                if (isExistPhone != null) return Json(new { status = "failed", data = "Số điện thoại " + register.Phone + " đã được đăng ký" });

                string salt = Utilities.GetRandomKey();
                Customer customer = new Customer
                {
                    Fullname = register.Fullname,
                    Email = register.Email.Trim().ToLower(),
                    Password = (register.Password + salt.Trim()).ToMD5(),
                    Phone = register.Phone.Trim().ToLower(),
                    Salt = salt,
                    CreateDate = DateTime.Now,
                    Avatar = "default-avatar.jpg"
                };
                _context.Add(customer);
                await _context.SaveChangesAsync();
                // Save session of customer
                HttpContext.Session.SetString("CustomerId", customer.Id.ToString());
                HttpContext.Session.SetString("CustomerName", customer.Fullname);
                HttpContext.Session.SetString("CustomerAvatar", customer.Avatar);

                // Identity
                var Claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, customer.Fullname),
                        new Claim("CustomerId", customer.Id.ToString())
                    };
                ClaimsIdentity claimsIdentity = new ClaimsIdentity(Claims, "Login");
                ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                await HttpContext.SignInAsync(claimsPrincipal);
            }
            return StatusCode(201);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("dang-xuat.html", Name = "Log out")]
        public IActionResult LogOutCustomer()
        {
            HttpContext.SignOutAsync();
            HttpContext.Session.Remove("CustomerId");
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("doi-mat-khau.html", Name = "ChangePassword")]
        public async Task<IActionResult> ChangePassword(ChangePasswordVM change)
        {
            var CustomerId = HttpContext.Session.GetString("CustomerId");
            if (CustomerId == null)
            {
                return RedirectToAction("CustomerLogin", "Customer");
            }
            if (ModelState.IsValid)
            {
                var account = await _context.Customers
                    .FindAsync(Convert.ToInt32(CustomerId));
                if (account == null) return RedirectToAction("CustomerLogin", "Customer");
                var currentPass = (change.PasswordNow.Trim() + account.Salt.Trim()).ToMD5();
                if (currentPass != account.Password) return Json(new { status = "failed", data = "Mật khẩu hiện tại của bạn nhập vào không đúng!" });
                var pass = change.ConfirmPassword;
                if (pass != change.Password) return Json(new { status = "failed", data = "Xác nhận mật khẩu thất bại, Mật khẩu xác nhận không khớp!" });
                var newPass = (change.Password.Trim() + account.Salt.Trim()).ToMD5();
                account.Password = newPass;
                _context.Update(account);
                _context.SaveChangesAsync();
                return RedirectToAction("GetProfile", "Customer");
            }
            return Json(new { status = "failed", data = "Thay đổi mật khẩu không thành công" });
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("trang-ca-nhan.html", Name = "Profile")]
        public async Task<IActionResult> GetProfile(int? page)
        {
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 10;

            var CustomerId = HttpContext.Session.GetString("CustomerId");
            if (CustomerId != null)
            {
                var Customer = await _context.Customers
                .AsNoTracking().SingleOrDefaultAsync(x => x.Id.ToString() == CustomerId);
                if (Customer != null)
                {
                    var listOrder = await _context.Orders
                        .AsNoTracking().Include(x => x.TransactionStatus)
                        .Where(x => x.CustomerId == Customer.Id)
                        .Where(x => x.Deleted == false)
                        .OrderByDescending(x => x.OrderDate)
                        .ToListAsync();

                    var listFavorite = await _context.Favorites
                        .AsNoTracking()
                        .Include(x => x.Product)
                        .Where(x => x.CustomerId == Customer.Id)
                        .OrderByDescending(x => x.Id)
                        .ToListAsync();

                    PagedList<Order> models = new PagedList<Order>(listOrder.AsQueryable(), pageNumber, pageSize);
                    PagedList<Favorite> modelsfavorite = new PagedList<Favorite>(listFavorite.AsQueryable(), pageNumber, pageSize);
                    ViewBag.Page = models;
                    ViewBag.PageFavorite = modelsfavorite;
                    ViewBag.CurrentPage = pageNumber;
                    ViewBag.OrderList = listOrder;
                    ViewBag.FavoriteList = listFavorite;
                    return View("ProfileCustomer", Customer);
                }

            }
            return RedirectToAction("CustomerLogin", "Customer");

        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateProfile(int customerid, [Bind("Id,Email,Password,Fullname,Phone,Dob,Avatar,CreateDate,Salt,Address,Deactivate")] Customer customer, IFormFile? Avatar, int? page)
        {
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 10;
            if (customerid != customer.Id)
            {
                return NotFound();
            }
            var Customer = await _context.Customers
                                       .AsNoTracking()
                                       .SingleOrDefaultAsync(x => x.Id == customerid);
            if (ModelState.IsValid)
            {
                try
                {
                    if (Customer != null)
                    {
                        var listOrder = await _context.Orders
                            .AsNoTracking().Include(x => x.TransactionStatus)
                            .Where(x => x.CustomerId == Customer.Id)
                            .Where(x => x.Deleted == false)
                            .OrderByDescending(x => x.OrderDate)
                            .ToListAsync();

                        var listFavorite = await _context.Favorites
                            .AsNoTracking()
                            .Include(x => x.Product)
                            .Where(x => x.CustomerId == Customer.Id)
                            .OrderByDescending(x => x.Id)
                            .ToListAsync();

                        PagedList<Order> models = new PagedList<Order>(listOrder.AsQueryable(), pageNumber, pageSize);
                        PagedList<Favorite> modelsfavorite = new PagedList<Favorite>(listFavorite.AsQueryable(), pageNumber, pageSize);
                        ViewBag.Page = models;
                        ViewBag.PageFavorite = modelsfavorite;
                        ViewBag.CurrentPage = pageNumber;
                        ViewBag.OrderList = listOrder;
                        ViewBag.FavoriteList = listFavorite;
                        _notifyService.Success("Cập nhật thông tin thành công");
                        return View("ProfileCustomer", Customer);
                    }
                }
                catch (Exception)
                {
                    _notifyService.Error("Cập nhật thông tin thất bại");
                    ViewBag.CurrentPage = pageNumber;
                    return View("ProfileCustomer", Customer);
                }
            }
            _notifyService.Error("Cập nhật thông tin thất bại");
            ViewBag.CurrentPage = pageNumber;
            return View("ProfileCustomer", Customer);
        }
    }
}
