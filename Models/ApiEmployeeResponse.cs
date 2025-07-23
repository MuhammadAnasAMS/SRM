using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PIA_Admin_Dashboard.Models
{
    public class ApiEmployeeResponse
    {
        public string pno { get; set; }
        public string name { get; set; }
        public string Emp_designation { get; set; }
        public string image_url { get; set; }
        public string email { get; set; }
        public string status { get; set; }
        public string description { get; set; }
        public string otp { get; set; }
        public string person_type { get; set; }
        public string Emp_NIC { get; set; }
        public string Department { get; set; }
        public string Phone_Num { get; set; }
        public string doj { get; set; }
        public string location { get; set; }
    }
}