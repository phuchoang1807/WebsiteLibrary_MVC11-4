namespace WebsiteLibrary.Models
{
    public class LoanCartItem
    {
        public string BookID { get; set; }        // FK tới Book.OriginalBookID
        public string Title { get; set; }         // Tiêu đề sách
        public int AvailableQuantity { get; set; } // Số lượng sẵn sàng
        public string Status => AvailableQuantity >= 1 ? "Sẵn sàng" : "Không sẵn sàng"; // Trạng thái
        public string ImagePath { get; set; }
    }
}
