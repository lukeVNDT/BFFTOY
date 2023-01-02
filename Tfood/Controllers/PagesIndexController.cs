using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tfood.Models;

namespace Tfood.Controllers
{
    public class PagesIndexController : Controller
    {
        private readonly tfoodContext _context;
        public PagesIndexController(tfoodContext context)
        {
            _context = context;
        }
        [Route("/pages-detail/{Alias}--{Id}.html", Name = "PagesDetail")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Pages == null)
            {
                return NotFound();
            }

            var Pages = await _context.Pages
                .FirstOrDefaultAsync(m => m.Id == id);



            if (Pages == null)
            {
                return NotFound();
            }
            return View("PagesDetails", Pages);
        }
    }
}
