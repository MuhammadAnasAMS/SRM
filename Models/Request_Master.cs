using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PIA_Admin_Dashboard.Models
{
    [Table("Request_Master")]
    public class Request_Master
    {
        [Key]
        [Column("RequestID")]
        public int RequestID { get; set; }

        [Column("RequestDate")]
        public DateTime? RequestDate { get; set; }

        [Column("RequestLogBy")]
        public string RequestLogBy { get; set; }

        [Column("RequestFor")]
        public string RequestFor { get; set; }

        [Column("Priority")]
        public int? Priority { get; set; }

        [Column("parea")]
        public string PArea { get; set; }

        [Column("status")]
        public string Status { get; set; }

        [Column("ReqSummary")]
        public string ReqSummary { get; set; }

        [Column("RequestedIPAddress")]
        public string RequestedIPAddress { get; set; }

        [Column("ReqDetails")]
        public string ReqDetails { get; set; }

        [Column("ForwardedDate")]
        public DateTime? ForwardedDate { get; set; }

        [Column("Forward_To")]
        public string ForwardTo { get; set; }

        [Column("ForwardedRemarks")]
        public string ForwardedRemarks { get; set; }

        [Column("ReqCloseDate")]
        public DateTime? ReqCloseDate { get; set; }

        [Column("ReqCloseBy")]
        public string ReqCloseBy { get; set; }

        [Column("Forward_By")]
        public string ForwardBy { get; set; }

        [Column("Forward_To_Type")]
        public string ForwardToType { get; set; }

        [Column("ownership")]
        public string Ownership { get; set; }

        [Column("uid")]
        public string UID { get; set; }

        [Column("Location")]
        public string Location { get; set; }

        [Column("ActualPArea")]
        public string ActualPArea { get; set; }

        [Column("ReqResolveDate")]
        public DateTime? ReqResolveDate { get; set; }

        [Column("ReqResolveBy")]
        public string ReqResolveBy { get; set; }

        [Column("program_id")]
        public int? ProgramId { get; set; }

        [Column("InitialRequest")]
        public string InitialRequest { get; set; }

        [Column("UnsatisfactoryClosed")]
        public string UnsatisfactoryClosed { get; set; }

        [Column("LogPortal")]
        public string LogPortal { get; set; }

        [NotMapped]
        public string Email { get; set; }
    }
}
