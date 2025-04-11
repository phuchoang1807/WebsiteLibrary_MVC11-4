namespace WebsiteLibrary.Models.Entities
{
    public class Reader : Account
    {
        public virtual ICollection<LibraryCard> LibraryCards { get; set; }
        // Liên kết tới các phiếu mượn của độc giả
        public virtual ICollection<BorrowingSlip> BorrowingSlips { get; set; }
        public string ReaderCode { get; set; }
    }
}
