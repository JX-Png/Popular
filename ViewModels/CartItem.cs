namespace PopularBookstore.ViewModels
{
    public class CartItem
    {
        public int BookId { get; set; }
        public int Quantity { get; set; }
        public string Title { get; set; }
        public decimal Price { get; set; }
        public byte[]? ImageData { get; set; }
        public string? ImageMimeType { get; set; }
    }
}