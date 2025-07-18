namespace PopularBookstore.ViewModels
{
    public class CartItem
    {
        public int BookId { get; set; }
        public int Quantity { get; set; }

        // You can add other properties if needed, like Price at the time of adding
        public string Title { get; set; }
        public decimal Price { get; set; }
        public byte[]? ImageData { get; set; }
        public string? ImageMimeType { get; set; }
    }
}