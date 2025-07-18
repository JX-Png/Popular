using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Data;
using WebApplication1.Models;

namespace PopularBookstore.Pages
{
    public class SearchModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public SearchModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Book> Books { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        public async Task OnGetAsync()
        {
            var booksQuery = from b in _context.Books
                             select b;

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                booksQuery = booksQuery.Where(s => s.Title.Contains(SearchTerm) || s.Author.Contains(SearchTerm));
            }

            // Step 1: Fetch the search results from the database
            var searchResults = await booksQuery.ToListAsync();

            // Step 2: Sort the results in memory using our custom logic
            Books = searchResults.OrderBy(b => GetSortableTitle(b.Title)).ToList();
        }

        // Helper method to get a title that can be sorted naturally
        private string GetSortableTitle(string title)
        {
            if (string.IsNullOrEmpty(title))
                return title;

            var articles = new[] { "The ", "A ", "An " };
            foreach (var article in articles)
            {
                if (title.StartsWith(article, StringComparison.OrdinalIgnoreCase))
                {
                    // If the title starts with an article, return the rest of the title
                    return title.Substring(article.Length);
                }
            }

            // Otherwise, return the original title
            return title;
        }
    }
}