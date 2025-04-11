using System.ComponentModel.DataAnnotations;
using NanoidDotNet;
using static NanoidDotNet.Nanoid;

namespace WebsiteLibrary.Models.Entities
{
    public class Account
    {
        [Key]
        public string ID { get; set; } = Nanoid.Generate(size: 10);
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        [Required]
        [StringLength(100)]
        public string Password { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Phone]
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string EducationLevel { get; set; }
        public string Gender { get; set; }
        [Required]
        public string Role { get; set; }
    }
}
