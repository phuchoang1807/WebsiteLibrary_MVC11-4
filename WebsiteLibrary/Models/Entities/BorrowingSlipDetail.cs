using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebsiteLibrary.Models.Entities
{
    public class BorrowingSlipDetail
    {
        // Khóa chính ghép
        [Key, Column(Order = 0)]
        public Guid BorrowingSlipId { get; set; } // Chỉ làm khóa chính

        [Key, Column(Order = 1)]
        public Guid BookCopyId { get; set; } // Chỉ làm khóa chính

        // Thuộc tính khóa ngoại riêng
        [ForeignKey("BorrowingSlip")]
        public Guid BorrowingSlipFK { get; set; } // Khóa ngoại trỏ đến BorrowingSlip

        [ForeignKey("BookCopy")]
        public Guid BookCopyFK { get; set; } // Khóa ngoại trỏ đến BookCopy

        public Condition Condition { get; set; } 

        public BorrowingSlip BorrowingSlip { get; set; }
        public BookCopy BookCopy { get; set; }
    }
}
