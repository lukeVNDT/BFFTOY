using System.ComponentModel.DataAnnotations;

namespace Tfood.ViewModel
{
    public class UpdateProfileVM
    {
        [Key]
        public int Id { get; set; }
        public string? Fullname { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public DateTime? Dob { get; set; }
        public string? Avatar { get; set; }
    }
}
