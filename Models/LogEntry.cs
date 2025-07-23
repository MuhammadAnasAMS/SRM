using System;

namespace PIA_Admin_Dashboard.Models
{
    public class LogEntry
    {
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string IpAddress { get; set; }
        public string Status { get; set; }
        public DateTime DateTime { get; set; }
    }
}
