using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebsiteLibrary.Models.Entities
{
    public class ImportDetail
    {
        [Key, Column(Order = 0)]
        public Guid ImportReceiptId { get; set; }

        [Key, Column(Order = 1)]
        public string OriginalBookId { get; set; } // Đã đổi từ Guid sang string

        [ForeignKey("ImportReceipt")]
        public Guid ImportReceiptFK { get; set; }

        [ForeignKey("Book")]
        public string OriginalBookFK { get; set; } // Đã đổi từ Guid sang string

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Range(0, int.MaxValue)]
        public int Price { get; set; }

        public ImportReceipt ImportReceipt { get; set; }
        public Book Book { get; set; }
    }
}