using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteLibrary.Models.Entities
{
    public class LibraryCardRequest
    {
        [Key]
        public string RequestID { get; set; } = NanoidDotNet.Nanoid.Generate(size: 10); // Khóa chính cho LibraryCardRequest

        [Required]
        public string AccountID { get; set; } // Khóa ngoại liên kết đến Account

        [ForeignKey("AccountID")]
        public Account Account { get; set; } // Navigation property

        [Required]
        public string ReaderName { get; set; } // Tên độc giả (lấy từ Account.Name)

        [Required]
        public DateTime RegistrationDate { get; set; } // Ngày đăng ký

        [Required]
        public DateTime ExpirationDate { get; set; } // Ngày hết hạn

        public string CardPhotoUrl { get; set; } // Đường dẫn ảnh thẻ
    }
}