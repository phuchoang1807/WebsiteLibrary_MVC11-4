using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteLibrary.Models.Entities
{
    public class LoanRequest

    {

        [Key]
        public Guid RequestID { get; set; }

        [Required]
        [ForeignKey("LibraryCard")]
        public string CardID { get; set; }
        public DateTime RequestDate { get; set; }
        
        public LoanRequestStatus Status { get; set; }
        public ICollection<LoanRequestDetail> LoanRequestDetails { get; set; }

    }
    public enum LoanRequestStatus

    {

        [Display(Name = "Chờ duyệt")]

        Choduyet,

        [Display(Name = "Đã duyệt")]

        Daduyet,

        [Display(Name = "Bị từ chối")]

        Bituchoi,

        [Display(Name = "Đã hủy")]

        Dahuy

    }
}
