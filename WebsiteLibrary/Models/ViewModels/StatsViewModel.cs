namespace WebsiteLibrary.Models.ViewModels
{
    public class StatsViewModel
    {
        // Hoạt động tháng này
        public int MonthlyBorrows { get; set; }
        public int MonthlyReturns { get; set; }

        // Tình trạng sách
        public int TotalBooks { get; set; }
        public int BorrowedBooks { get; set; }

        // Hoạt động trả sách
        public int TotalReturns { get; set; }
        public int OverdueBooks { get; set; }

        // Thống kê lượt mượn/lượt trả sách (biểu đồ đường)
        public int[] BorrowData { get; set; }
        public int[] ReturnData { get; set; }

        // Thống kê sách theo thể loại (biểu đồ cột)
        public Dictionary<string, int> BooksByCategory { get; set; }

        // Thể loại sách được mượn nhiều (biểu đồ tròn)
        public Dictionary<string, double> BorrowedBooksByCategory { get; set; }

        // Danh sách phiếu cần bồi thường
        public List<CompensationViewModel> Compensations { get; set; }
    }

    public class CompensationViewModel
    {
        public string BorrowingSlipId { get; set; }
        public string ReaderName { get; set; }
        public string PhoneNumber { get; set; }
        public int DamagedBooks { get; set; }
        public int LostBooks { get; set; }
        public decimal CompensationAmount { get; set; }
    }
}