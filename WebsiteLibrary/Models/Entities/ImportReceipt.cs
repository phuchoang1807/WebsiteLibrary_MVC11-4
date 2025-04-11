using System.ComponentModel.DataAnnotations;

namespace WebsiteLibrary.Models.Entities
{
    public class ImportReceipt
    {

        [Key]

        public Guid ImportReceiptID { get; set; }

        public DateTime ImportDate { get; set; }

        [StringLength(100)]
        public string Supplier { get; set; }

        public ICollection<ImportDetail> ImportDetails { get; set; }


    }
}
