using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebsiteLibrary.Models.Entities
{
    public class ReturnSlipDetail
    {
        [Key, Column(Order = 0)]
        public Guid ReturnSlipId { get; set; }

        [Key, Column(Order = 1)]
        public Guid BookId { get; set; } 

        [ForeignKey("ReturnSlip")]
        public Guid ReturnSlipFK { get; set; }

        [ForeignKey("BookCopy")]
        public Guid BookCopyFK { get; set; }

        public Condition Condition { get; set; }

        public ReturnSlip ReturnSlip { get; set; }
        public BookCopy BookCopy { get; set; }
    }
}