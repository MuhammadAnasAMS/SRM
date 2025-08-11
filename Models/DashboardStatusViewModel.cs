using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PIA_Admin_Dashboard.Models
{
    public class DashboardStatusViewModel
    {
        public int Queue { get; set; }
        public int Forwarded { get; set; }
        public int In_Progress { get; set; }
        public int Resolved { get; set; }
        public int Closed { get; set; }
        public int Total => Queue + Forwarded + Resolved + Closed + In_Progress;
    }
}