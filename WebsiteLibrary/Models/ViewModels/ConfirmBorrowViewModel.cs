namespace WebsiteLibrary.ViewModels
{
    public class ConfirmBorrowViewModel
    {
        public WebsiteLibrary.Models.Entities.Reader Reader { get; set; }
        public List<WebsiteLibrary.Models.LoanCartItem> CartItems { get; set; }
        public DateTime BorrowDeadline { get; set; }
    }
}