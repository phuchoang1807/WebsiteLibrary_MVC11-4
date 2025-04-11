using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebsiteLibrary.Models.Entities
{
    public class ReturnSlip

    {

        [Key]

        public Guid ReturnSlipID { get; set; }

        [Required]
        [ForeignKey("BorrowingSlip")]

        public Guid BorrowingSlipID { get; set; }
        public DateTime ReturnDate { get; set; }
        public BorrowingSlip BorrowingSlip { get; set; }

        public ICollection<ReturnSlipDetail> ReturnSlipDetails { get; set; }

    }
}
