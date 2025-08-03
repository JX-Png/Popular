using System;

namespace PopularBookstore.ViewModels
{
    public class CartItemViewModel
    {
        public int BookId { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; }
        public byte[] ImageData { get; set; }
        public string ImageMimeType { get; set; }

        public decimal Total => Price * Quantity;
    }
}