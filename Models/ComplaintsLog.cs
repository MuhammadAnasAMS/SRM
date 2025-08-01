using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PIA_Admin_Dashboard.Models
{
    public class ComplaintsLog
    {
        public List<RequestItem> Requests { get; set; }
        public ComplaintStats Stats { get; set; }

        public int OpenRequests { get; set; }         // <-- Add this
        public int PendingRequests { get; set; }      // <-- Add this

        // Filters
        public string SearchBy { get; set; }
        public string SearchValue { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string OrderBy { get; set; }
        public string Status { get; set; }
    }

    public class RequestItem
    {
        public int RequestID { get; set; }
        public string Subject { get; set; }
        public DateTime RequestDate { get; set; }
        public string ForwardTo { get; set; }
        public string Site { get; set; }
        public string Status { get; set; }
        public string PendingSince { get; set; }
    }

    public class ComplaintStats
    {
        public int Queue { get; set; }
        public int Forwarded { get; set; }
        public int Resolved { get; set; }
        public int Closed { get; set; }
        public int Total => Queue + Forwarded + Resolved + Closed;
    }
}