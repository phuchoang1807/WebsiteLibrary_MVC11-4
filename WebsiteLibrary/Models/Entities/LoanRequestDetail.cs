using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebsiteLibrary.Models.Entities
{
    public class LoanRequestDetail

    {

        [Key, Column(Order = 0)]
        public Guid RequestId { get; set; } // Khóa chính

        [Key, Column(Order = 1)]
        public Guid BookCopyId { get; set; } // Khóa chính

        [ForeignKey("LoanRequest")]
        public Guid RequestFK { get; set; } // Khóa ngoại riêng

        [ForeignKey("BookCopy")]
        public Guid BookCopyFK { get; set; } // Khóa ngoại riêng
        public LoanRequest LoanRequest { get; set; }
        public BookCopy BookCopy { get; set; }

    }
}
