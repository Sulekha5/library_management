namespace LibraryManagement.Models
{
    public class Book
    {
        public string Name { get; set; } = string.Empty;
        public int BookId { get; set; }
        public string Author { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}