﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tfood.Models;

namespace Tfood.Controllers.Components
{
    public class FavoriteIndexViewComponent : ViewComponent
    {
        private readonly tfoodContext _context;

        public FavoriteIndexViewComponent(tfoodContext context)
        {
            _context = context;
        }
        public IViewComponentResult Invoke()
        {
            var favorite = _context.Favorites
                .AsNoTracking()
                .Include(x => x.Product)
                .ToList();
            return View(favorite);
        }
    }
}
