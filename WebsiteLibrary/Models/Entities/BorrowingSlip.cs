using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebsiteLibrary.Models.Entities
{
    public class BorrowingSlip
    {
        [Key]
        public Guid BorrowingSlipID { get; set; }

        [Required]

        [ForeignKey("LibraryCard")]

        public string CardID { get; set; }
        public DateTime BorrowDate { get; set; }

        [StringLength(50)]
        public string BorrowDuration { get; set; }
        public BorrowingSlipStatus Status { get; set; }
        public ICollection<BorrowingSlipDetail> BorrowingSlipDetails { get; set; }
        public virtual LibraryCard LibraryCard { get; set; } // Thêm thuộc tính điều hướng
        

    }
    public enum BorrowingSlipStatus

    {

        [Display(Name = "Đang mượn")]

        Dangmuon,

        [Display(Name = "Đã trả")]

        Datra,

        [Display(Name = "Quá hạn")]

        Quahan

    }
}
