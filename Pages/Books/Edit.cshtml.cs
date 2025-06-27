using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication1.Data;
using WebApplication1.Models;
using System.Linq;

public class BooksEditModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public BooksEditModel(ApplicationDbContext context) => _context = context;

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
        if (!ModelState.IsValid)
            return Page();

        var bookInDb = _context.Books.FirstOrDefault(b => b.Id == Book.Id);
        if (bookInDb == null) return RedirectToPage("Index");

        bookInDb.Title = Book.Title;
        bookInDb.Author = Book.Author;
        bookInDb.Price = Book.Price;
        bookInDb.Genre = Book.Genre;
        bookInDb.ImageUrl = Book.ImageUrl;

        _context.SaveChanges();
        return RedirectToPage("Index");
    }
}