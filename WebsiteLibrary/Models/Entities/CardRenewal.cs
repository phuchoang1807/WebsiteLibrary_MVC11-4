using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebsiteLibrary.Models.Entities
{
    public class CardRenewal
    {
        [Key]
        public string RenewalID { get; set; }
        public string CardID { get; set; }
        public DateTime RenewalDate { get; set; }
        public DateTime NewExpirationDate { get; set; }
        public string Status { get; set; }

        [ForeignKey("CardID")]
        public virtual LibraryCard LibraryCard { get; set; }
    }
}
