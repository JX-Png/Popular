using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IO;
using System.Threading.Tasks;
using WebApplication1.Data;
using WebApplication1.Models;
using System.Linq;

namespace PopularBookstore.Pages.Books
{
    [Authorize(Roles = "Admin")]

    public class BooksEditModel : PageModel  // This must match the model in Edit.cshtml
    {
        private readonly ApplicationDbContext _context;

        public BooksEditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Book Book { get; set; }

        [BindProperty]
        public IFormFile UploadedImage { get; set; }

        public IActionResult OnGet(int id)
        {
            Book = _context.Books.FirstOrDefault(b => b.Id == id);
            if (Book == null) return RedirectToPage("Index");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var bookInDb = _context.Books.FirstOrDefault(b => b.Id == Book.Id);
            if (bookInDb == null) return RedirectToPage("Index");

            bookInDb.Title = Book.Title;
            bookInDb.Author = Book.Author;
            bookInDb.Description = Book.Description;
            bookInDb.Price = Book.Price;
            bookInDb.Genre = Book.Genre;

            // Handle image URL if provided and no new image uploaded
            if (!string.IsNullOrWhiteSpace(Book.ImageUrl) &&
                (UploadedImage == null || UploadedImage.Length == 0))
            {
                bookInDb.ImageUrl = Book.ImageUrl;
                bookInDb.ImageData = null;
                bookInDb.ImageMimeType = null;
            }

            // Process image upload if a new image was provided
            if (UploadedImage != null && UploadedImage.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await UploadedImage.CopyToAsync(memoryStream);
                    bookInDb.ImageData = memoryStream.ToArray();
                    bookInDb.ImageMimeType = UploadedImage.ContentType;
                    bookInDb.ImageUrl = null; // Clear URL if we have image data
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToPage("Index");
        }
    }
}