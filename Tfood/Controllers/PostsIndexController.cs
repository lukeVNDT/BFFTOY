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

namespace Tfood.Controllers
{
    public class PostsIndexController : Controller
    {
        private readonly tfoodContext _context;

        public PostsIndexController(tfoodContext context)
        {
            _context = context;
        }

        // GET: PostsIndex
        [Route("posts.html", Name = "Posts")]
        public async Task<IActionResult> Index(int page = 1)
        {
            var pageNumber = page;
            var pageSize = 8;
            List<Post> listPost = new List<Post>();
            listPost = _context.Posts.AsNoTracking()
               .Include(x => x.Account)
               .Include(x => x.Cat)
               .OrderByDescending(x => x.Id)
               .ToList();

            PagedList<Post> models = new PagedList<Post>(listPost.AsQueryable(), pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            return View(models);
        }

        // GET: PostsIndex/Details/5
        [Route("/post-detail/{Alias}--{Id}.html", Name = "PostsDetail")]
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
            var relationalPost = await _context.Posts
                .AsNoTracking()
                .Include(p => p.Cat)
                .Where(p => p.Published == true && p.Id != id)
                .Take(3)
                .OrderByDescending(x => x.CreateDate)
                .ToListAsync();
            ViewBag.RelationalPost = relationalPost;
            return View(post);
        }

        // GET: PostsIndex/Create
        public IActionResult Create()
        {
            ViewData["AccountId"] = new SelectList(_context.Accounts, "Id", "Id");
            ViewData["CatId"] = new SelectList(_context.PostCategories, "Id", "Id");
            return View();
        }

        // POST: PostsIndex/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,ShortDesc,Contents,Thumb,CreateDate,Published,Author,AccountId,IsHot,IsNewFeed,CatId,Alias,MetaDesc,MetaKey")] Post post)
        {
            if (ModelState.IsValid)
            {
                _context.Add(post);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AccountId"] = new SelectList(_context.Accounts, "Id", "Id", post.AccountId);
            ViewData["CatId"] = new SelectList(_context.PostCategories, "Id", "Id", post.CatId);
            return View(post);
        }

        // GET: PostsIndex/Edit/5
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
            ViewData["CatId"] = new SelectList(_context.PostCategories, "Id", "Id", post.CatId);
            return View(post);
        }

        // POST: PostsIndex/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,ShortDesc,Contents,Thumb,CreateDate,Published,Author,AccountId,IsHot,IsNewFeed,CatId,Alias,MetaDesc,MetaKey")] Post post)
        {
            if (id != post.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(post);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["AccountId"] = new SelectList(_context.Accounts, "Id", "Id", post.AccountId);
            ViewData["CatId"] = new SelectList(_context.PostCategories, "Id", "Id", post.CatId);
            return View(post);
        }

        // GET: PostsIndex/Delete/5
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

        // POST: PostsIndex/Delete/5
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
