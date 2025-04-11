namespace WebsiteLibrary.Models.ViewModels
{
    public class DashboardViewModel
    {
        // Hoạt động hôm nay
        public int TodayBorrows { get; set; }
        public int TodayReturns { get; set; }

        // Tình trạng sách
        public int TotalBooks { get; set; }
        public int BorrowedBooks { get; set; }

        // Đặt mượn
        public int PendingReservations { get; set; }
        public int OverdueBooks { get; set; }

        // Dữ liệu biểu đồ
        public int[] BorrowData { get; set; }
        public int[] ReturnData { get; set; }

        // Hoạt động gần đây
        public List<RecentActivityViewModel> RecentActivities { get; set; }
    }

    public class RecentActivityViewModel
    {
        public string BookTitle { get; set; }
        public string Category { get; set; }
        public string ReaderName { get; set; }
        public DateTime BorrowDate { get; set; }
    }
}
