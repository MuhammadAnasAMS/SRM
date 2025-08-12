using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace PIA_Admin_Dashboard.Models
{
    using System.ComponentModel.DataAnnotations;
    public class OtpModel
    {
        [Required(ErrorMessage = "Employee ID is required")]
        [Display(Name = "Employee ID")]
        public string EmployeeId { get; set; }

        [Required(ErrorMessage = "Confirm Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [Display(Name = "Confirm Email")]
        public string ConfirmEmail { get; set; }
    }
}