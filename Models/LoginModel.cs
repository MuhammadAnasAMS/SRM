using System.ComponentModel.DataAnnotations;

namespace PIA_Admin_Dashboard.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        // Optional: Captured automatically in controller, not the view
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
    }
}
