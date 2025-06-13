using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PopularBookstore.Pages
{
    public class IndexModel : PageModel
    {
        public List<Book> Books { get; set; } = new List<Book>();

        public void OnGet()
        {
            // Example data for demonstration
            Books.Add(new Book { Id = 1, Title = "Book 1", ImageUrl = "/images/book1.jpg" });
            Books.Add(new Book { Id = 2, Title = "Book 2", ImageUrl = "/images/book2.jpg" });
        }
    }

    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ImageUrl { get; set; }
    }
}