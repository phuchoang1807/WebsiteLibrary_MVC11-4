using WebsiteLibrary.Models.Entities;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace WebsiteLibrary.ViewModels
{
    public class ReturnSlipViewModel
    {
        [Required]
        public Guid BorrowingSlipID { get; set; }

        [Required]
        public DateTime ReturnDate { get; set; }

        [Required]
        public List<ReturnDetailViewModel> ReturnDetails { get; set; }
    }

    public class ReturnDetailViewModel
    {
        [Required]
        public Guid BookCopyId { get; set; }

        [Required]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Condition Condition { get; set; }
    }
}