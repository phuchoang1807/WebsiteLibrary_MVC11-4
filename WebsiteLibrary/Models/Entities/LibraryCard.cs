using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebsiteLibrary.Models.Entities
{
    public class LibraryCard
    {
        [Key]
        public string CardID { get; set; }
        public string ID { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string Status { get; set; }
        public string CardPhotoUrl { get; set; }

        [ForeignKey("ID")]
        public virtual Reader Reader { get; set; }
    }
}
