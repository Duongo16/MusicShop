using System.ComponentModel.DataAnnotations;

namespace MusicShop.Common.ViewModels
{
    public class ProfileViewModel
    {
        public Guid Id { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty; 

        [Display(Name = "User name")]
        public string? UserName { get; set; }

        [Display(Name = "Full name")]
        public string? FullName { get; set; }

        [Phone]
        [Display(Name = "Phone")]
        public string? Phone { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date of birth")]
        public DateTime? Dob { get; set; }

        [Display(Name = "Address")]
        public string? Address { get; set; }
    }

}
