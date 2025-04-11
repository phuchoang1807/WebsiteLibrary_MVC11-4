using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteLibrary.Models.Entities
{
    public class Payment
    {
        public string PaymentID { get; set; }
        public string CardID { get; set; }
        public DateTime PaymentDate { get; set; }
        public float Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string Status { get; set; }

        [ForeignKey("CardID")]
        public virtual LibraryCard LibraryCard { get; set; }
    }
}
