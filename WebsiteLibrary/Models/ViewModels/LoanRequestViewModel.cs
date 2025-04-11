using WebsiteLibrary.Models.Entities;

namespace WebsiteLibrary.ViewModels
{
    public class LoanRequestViewModel
    {
        public Guid RequestID { get; set; }
        public string CardID { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public LoanRequestStatus Status { get; set; }
        public List<LoanRequestDetailViewModel> Details { get; set; }
    }
    public class LoanRequestDetailViewModel
    {
        public Guid BookCopyId { get; set; }
        public string BookTitle { get; set; } //  Lấy từ Book.Title qua BookCopy
    }
}