using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using WebApplication1.Data;
using WebApplication1.Models;

namespace PopularBookstore.Pages.Books
{
    [Authorize(Roles = "Admin")]

    public class BooksDeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public BooksDeleteModel(ApplicationDbContext context) => _context = context;

        [BindProperty]
        public Book Book { get; set; }

        public IActionResult OnGet(int id)
        {
            Book = _context.Books.FirstOrDefault(b => b.Id == id);
            if (Book == null) return RedirectToPage("Index");
            return Page();
        }

        public IActionResult OnPost()
        {
            var bookInDb = _context.Books.FirstOrDefault(b => b.Id == Book.Id);
            if (bookInDb != null)
            {
                _context.Books.Remove(bookInDb);
                _context.SaveChanges();
            }
            return RedirectToPage("Index");
        }
    }
}