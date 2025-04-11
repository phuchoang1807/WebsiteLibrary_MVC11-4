using System.ComponentModel.DataAnnotations;

namespace WebsiteLibrary.ViewModels
{
    public class CreateBorrowingSlipViewModel
    {
        [Required]
        public string CardID { get; set; }

        [Required]
        public DateTime BorrowDate { get; set; }

        [Required]
        public string BorrowDuration { get; set; }

        [Required]
        public List<string> BookIds { get; set; }
    }
}