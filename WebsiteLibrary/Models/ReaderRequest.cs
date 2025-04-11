namespace WebsiteLibrary.Models.Entities
{
    public class ReaderRequest
    {
        public string Name { get; set; }
        public string DateOfBirth { get; set; } // Chuỗi YYYY-MM-DD
        public string Gender { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string EducationLevel { get; set; }
    }
}