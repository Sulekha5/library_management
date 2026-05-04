namespace LibraryManagement.Models
{
    public class IssueRequest
    {
        public string StudentName { get; set; }= string.Empty;
        public string BookName { get; set; }=string.Empty;
        public DateTime ReturnDate { get; set; }= DateTime.MinValue;

        public string Email { get; set; } = string.Empty;
        
    }

}