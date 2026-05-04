namespace LibraryManagement.Models
{
    public class IssuedBook
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int BookId { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public string Status { get; set; } = string.Empty;

        public Student? Student { get; set; }
        public Book? Book { get; set; } 
    }
}