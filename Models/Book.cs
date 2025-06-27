using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models // Or PopularBookstore.Models if you've changed it
{
    public class Book
    {
        public int Id { get; set; } // Primary Key

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        [StringLength(100)]
        public string Author { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18, 2)")] // Ensures proper decimal storage in DB
        public decimal Price { get; set; }

        [StringLength(50)] 
        public string? Genre { get; set; } 

        public string? ImageUrl { get; set; }

        public byte[]? ImageData { get; set; }
        public string? ImageMimeType { get; set; }
    }
}