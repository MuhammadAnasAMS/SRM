using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PIA_Admin_Dashboard.Models
{
    public class ComplaintFormViewModel
    {
        public string pno { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string ProgramName { get; set; }     // Program name
        public string Location { get; set; }   // Location description
        public int ProgramId { get; set; }
        public string ReqSummary { get; set; }
        public string ReqDetails { get; set; }
        public string Priority { get; set; }
        public List<string> ProblemAreas { get; set; } = new List<string>();
        public bool IsDetailsFetched { get; set; }
        public string RequestedIPAddress { get; set; }
    }

}