using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication1.Data;
using WebApplication1.Models;
using System.IO;
using System.Threading.Tasks;

namespace PopularBookstore.Pages.Books
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public CreateModel(ApplicationDbContext context) => _context = context;

        [BindProperty]
        public Book Book { get; set; } = new();

        [BindProperty]
        public IFormFile? ImageFile { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            if (ImageFile != null && ImageFile.Length > 0)
            {
                using var ms = new MemoryStream();
                await ImageFile.CopyToAsync(ms);
                Book.ImageData = ms.ToArray();
                Book.ImageMimeType = ImageFile.ContentType;
            }

            _context.Books.Add(Book);
            await _context.SaveChangesAsync();
            return RedirectToPage("Index");
        }
    }
}