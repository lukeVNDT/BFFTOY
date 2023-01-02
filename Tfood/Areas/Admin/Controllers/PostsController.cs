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
using Tfood.Helpper;
using Tfood.Models;

namespace Tfood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class PostsController : Controller
    {
        private readonly tfoodContext _context;
        public INotyfService _notifyService { get; }
        public PostsController(tfoodContext context, INotyfService notifyService)
        {
            _context = context;
            _notifyService = notifyService;
        }


        // GET: Admin/Posts/Filter
        public IActionResult Filter(int cateid = 0)
        {
            var url = $"/Admin/Posts?cateid={cateid}";
            if (cateid == 0)
            {
                url = "/Admin/Posts";
            }
            return Json(new { status = "success", redirectUrl = url });
        }
        // GET: Admin/Posts
        public async Task<IActionResult> Index(int cateid = 0)
        {
            var pageNumber = 1;
            var pageSize = 10;
            List<Post> listPost = new List<Post>();
            if (cateid != 0)
            {
                listPost = _context.Posts.AsNoTracking()
               .Include(x => x.Cat)
               .Where(x => x.CatId == cateid)
               .OrderByDescending(x => x.Id)
               .ToList();
            }
            else
            {
                listPost = _context.Posts.AsNoTracking()
               .Include(x => x.Cat)
               .OrderByDescending(x => x.Id)
               .ToList();
            }    
            

            PagedList<Post> models = new PagedList<Post>(listPost.AsQueryable(), pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            ViewData["Category"] = new SelectList(_context.PostCategories, "Id", "Name", cateid);
            return View(models);
        }

        // GET: Admin/Posts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Account)
                .Include(p => p.Cat)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // GET: Admin/Posts/Create
        public IActionResult Create()
        {
            ViewData["Cate"] = new SelectList(_context.PostCategories, "Id", "Name");
            ViewData["AccountId"] = new SelectList(_context.Accounts, "Id", "Id");
            return View();
        }

        // POST: Admin/Posts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,ShortDesc,Contents,Thumb,CreateDate,Published,Author,AccountId,IsHot,IsNewFeed,CatId,Alias,MetaDesc,MetaKey")] Post post, IFormFile? PostThumb)
        {
            if (ModelState.IsValid)
            {
                var AccountId = HttpContext.Session.GetString("AccountId");
                if (AccountId == null)
                {
                    AccountId = User.Identity.GetAccountID();
                }
                post.Title = Utilities.ToTitleCase(post.Title);
                if (PostThumb != null)
                {
                    string extension = Path.GetExtension(PostThumb.FileName);
                    string image = Utilities.SEOUrl(post.Title) + extension;
                    post.Thumb = await Utilities.UploadFile(PostThumb, @"posts", image.ToLower());
                }
                if (PostThumb == null) post.Thumb = "";
                post.Alias = Utilities.SEOUrl(post.Title);
                post.CreateDate = DateTime.Now;
                post.AccountId = Convert.ToInt32(AccountId);

                _context.Add(post);
                await _context.SaveChangesAsync();
                _notifyService.Success("Tạo bài viết thành công");
                return RedirectToAction(nameof(Index));
            }
            ViewData["AccountId"] = new SelectList(_context.Accounts, "Id", "Id", post.AccountId);
            ViewData["CatId"] = new SelectList(_context.PostCategories, "Id", "Id", post.CatId);
            _notifyService.Error("Tạo bài viết thất bại");
            return View(post);
        }

        // GET: Admin/Posts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }
            ViewData["AccountId"] = new SelectList(_context.Accounts, "Id", "Id", post.AccountId);
            ViewData["Cate"] = new SelectList(_context.PostCategories, "Id", "Name", post.CatId);
            return View(post);
        }

        // POST: Admin/Posts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,ShortDesc,Contents,Thumb,CreateDate,Published,Author,AccountId,IsHot,IsNewFeed,CatId,Alias,MetaDesc,MetaKey")] Post post, IFormFile? PostThumb)
        {
            if (id != post.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    post.Title = Utilities.ToTitleCase(post.Title);
                    if (PostThumb != null)
                    {
                        string extension = Path.GetExtension(PostThumb.FileName);
                        string image = Utilities.SEOUrl(post.Title) + extension;
                        post.Thumb = await Utilities.UploadFile(PostThumb, @"posts", image.ToLower());
                    }
                    post.Alias = Utilities.SEOUrl(post.Title);

                    _context.Update(post);
                    await _context.SaveChangesAsync();
                    _notifyService.Success("Cập nhật bài viết thành công");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    _notifyService.Success("Cập nhật bài viết thất bại, vui lòng kiểm tra lại");
                    return RedirectToAction(nameof(Index));
                }
            }
            ViewData["AccountId"] = new SelectList(_context.Accounts, "Id", "Id", post.AccountId);
            ViewData["CatId"] = new SelectList(_context.PostCategories, "Id", "Id", post.CatId);
            _notifyService.Success("Cập nhật bài viết thất bại");
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Posts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Account)
                .Include(p => p.Cat)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // POST: Admin/Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Posts == null)
            {
                return Problem("Entity set 'tfoodContext.Posts'  is null.");
            }
            var post = await _context.Posts.FindAsync(id);
            if (post != null)
            {
                _context.Posts.Remove(post);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PostExists(int id)
        {
          return _context.Posts.Any(e => e.Id == id);
        }
    }
}
