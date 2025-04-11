namespace WebsiteLibrary.Models
{
    public class LoanRequestDetailsResponse
    {
        public bool Success { get; set; }
        public string Status { get; set; }
        public string RequestDate { get; set; }
        public List<BookItem> Books { get; set; }
        public BorrowingSlipDetails BorrowingSlip { get; set; }
    }

    public class BookItem
    {
        public int Stt { get; set; }
        public string BookTitle { get; set; }
    }

    public class BorrowingSlipDetails
    {
        public string BorrowDate { get; set; }
        public string ReturnDate { get; set; }
        public string DueDate { get; set; }
        public string Status { get; set; }
    }
}