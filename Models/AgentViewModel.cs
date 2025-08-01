using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PIA_Admin_Dashboard.Models
{
    public class AgentViewModel
    {
        public Agent Agent { get; set; }

        // For radio/toggle selection (e.g., select by dropdown or not)
        public string RoleSelectionType { get; set; }

        // List for dropdown
        public List<SelectListItem> Roles { get; set; }

        // List for location dropdown
        public List<SelectListItem> Locations { get; set; }
    }
}