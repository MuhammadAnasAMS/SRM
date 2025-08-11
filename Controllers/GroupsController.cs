using System;
using System.Linq;
using System.Web.Mvc;
using System.Collections.Generic;
using PIA_Admin_Dashboard.Models;
using System.Data.Entity;

namespace PIA_Admin_Dashboard.Controllers
{
    public class GroupsController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        // Display all groups
        public ActionResult Index()
        {
            ViewBag.ScreenTitle = "Manage Groups";

            // Get all groups with their related data using anonymous object first
            var groupsData = (from g in db.Groups
                              join p in db.Programs on g.ProgramId equals p.ProgramId into programJoin
                              from program in programJoin.DefaultIfEmpty()
                              join a in db.Agents on g.ManagerPno equals a.PNO into agentJoin
                              from agent in agentJoin.DefaultIfEmpty()
                              select new
                              {
                                  Gid = g.Gid,
                                  GroupName = g.GroupName,
                                  ProgramId = g.ProgramId,
                                  Uid = g.Uid,
                                  ManagerPno = g.ManagerPno,
                                  ProgramName = program != null ? program.Name : null,
                                  ManagerName = agent != null ? agent.Name : null
                              }).ToList();

            // Map to Group objects
            var groups = groupsData.Select(g => new Group
            {
                Gid = g.Gid,
                GroupName = g.GroupName,
                ProgramId = g.ProgramId,
                Uid = g.Uid,
                ManagerPno = g.ManagerPno,
                ManagerName = g.ManagerName,
                Program = g.ProgramName != null ? new Program { Name = g.ProgramName } : null
            }).ToList();

            // Calculate member count for each group
            foreach (var group in groups)
            {
                group.MemberCount = db.Database.SqlQuery<int>(
                    "SELECT COUNT(*) FROM AgentGroup WHERE gid = @p0", group.Gid).FirstOrDefault();
            }

            // Populate dropdowns for adding new group
            ViewBag.Agents = db.Agents.Where(a => a.Status == "A") // Only active agents
                .Select(a => new SelectListItem
                {
                    Value = a.PNO,
                    Text = a.Name + " (" + a.PNO + ")"
                }).ToList();

            ViewBag.Programs = db.Programs.Select(p => new SelectListItem
            {
                Value = p.ProgramId.ToString(),
                Text = p.Name
            }).ToList();

            return View("~/Views/Admin/Groups/Index.cshtml", groups);
        }

        // GET: View Group Members
        public ActionResult ViewMembers(int id)
        {
            // Get group details with program name using anonymous object first
            var groupData = (from g in db.Groups
                             join p in db.Programs on g.ProgramId equals p.ProgramId into programJoin
                             from program in programJoin.DefaultIfEmpty()
                             where g.Gid == id
                             select new
                             {
                                 Gid = g.Gid,
                                 GroupName = g.GroupName,
                                 ProgramId = g.ProgramId,
                                 Uid = g.Uid,
                                 ManagerPno = g.ManagerPno,
                                 ProgramName = program != null ? program.Name : null
                             }).FirstOrDefault();

            if (groupData == null)
            {
                return HttpNotFound();
            }

            // Map to Group object
            var groupWithProgram = new Group
            {
                Gid = groupData.Gid,
                GroupName = groupData.GroupName,
                ProgramId = groupData.ProgramId,
                Uid = groupData.Uid,
                ManagerPno = groupData.ManagerPno,
                Program = groupData.ProgramName != null ? new Program { Name = groupData.ProgramName } : null
            };

            // Get manager name if exists
            if (!string.IsNullOrEmpty(groupWithProgram.ManagerPno))
            {
                var manager = db.Agents.FirstOrDefault(a => a.PNO == groupWithProgram.ManagerPno);
                if (manager != null)
                {
                    groupWithProgram.ManagerName = manager.Name;
                }
            }

            // Get group members using SQL query and then map to strongly typed objects
            var memberData = db.Database.SqlQuery<AgentGroupResult>(
                "SELECT ag.sno, ag.pno, ag.Name, ag.gid FROM AgentGroup ag WHERE ag.gid = @p0", id).ToList();

            var members = new List<GroupMember>();

            foreach (var memberItem in memberData)
            {
                var member = new GroupMember
                {
                    Sno = memberItem.sno,
                    Pno = memberItem.pno,
                    Name = memberItem.Name,
                    Gid = memberItem.gid
                };

                // Get additional agent details from Agent table
                var agent = db.Agents.FirstOrDefault(a => a.PNO == member.Pno);
                if (agent != null)
                {
                    member.Email = agent.Email;
                    member.Mobile = agent.Mobile;
                    member.Status = agent.Status;
                    member.WorkArea = agent.WorkArea;
                    member.LastLogin = agent.LastLoginDateTime;
                }

                members.Add(member);
            }

            ViewBag.Group = groupWithProgram;
            ViewBag.ScreenTitle = $"Group Members - {groupWithProgram.GroupName}";

            return View("~/Views/Admin/Groups/ViewMembers.cshtml", members);
        }

        // POST: Add new group
        [HttpPost]
        public ActionResult Add(string GroupName, string ManagerPno, int? ProgramId)
        {
            if (!string.IsNullOrWhiteSpace(GroupName))
            {
                // Check if group name already exists for the same program
                var exists = db.Groups.Any(g => g.GroupName.ToLower() == GroupName.ToLower()
                                              && g.ProgramId == ProgramId);

                if (exists)
                {
                    TempData["ErrorMessage"] = "Group name already exists for this program.";
                    return RedirectToAction("Index");
                }

                var group = new Group
                {
                    GroupName = GroupName,
                    ManagerPno = ManagerPno,
                    ProgramId = ProgramId,
                    Uid = Guid.NewGuid().ToString()
                };

                db.Groups.Add(group);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Group added successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Group name is required.";
            }

            return RedirectToAction("Index");
        }

        // GET: Edit form
        public ActionResult Edit(int id)
        {
            var group = db.Groups.Find(id);
            if (group == null)
            {
                return HttpNotFound();
            }

            // Get manager details if exists
            if (!string.IsNullOrEmpty(group.ManagerPno))
            {
                var manager = db.Agents.FirstOrDefault(a => a.PNO == group.ManagerPno);
                if (manager != null)
                {
                    group.ManagerName = manager.Name;
                }
            }

            ViewBag.Agents = db.Agents.Where(a => a.Status == "A") // Only active agents
                .Select(a => new SelectListItem
                {
                    Value = a.PNO,
                    Text = a.Name + " (" + a.PNO + ")",
                    Selected = a.PNO == group.ManagerPno
                }).ToList();

            ViewBag.Programs = db.Programs.Select(p => new SelectListItem
            {
                Value = p.ProgramId.ToString(),
                Text = p.Name,
                Selected = p.ProgramId == group.ProgramId
            }).ToList();

            return View("~/Views/Admin/Groups/Edit.cshtml", group);
        }

        // POST: Edit update
        [HttpPost]
        public ActionResult Edit(Group updatedGroup)
        {
            if (ModelState.IsValid)
            {
                // Check if group name already exists for the same program (excluding current group)
                var exists = db.Groups.Any(g => g.GroupName.ToLower() == updatedGroup.GroupName.ToLower()
                                              && g.ProgramId == updatedGroup.ProgramId
                                              && g.Gid != updatedGroup.Gid);

                if (exists)
                {
                    TempData["ErrorMessage"] = "Group name already exists for this program.";

                    // Re-populate dropdowns
                    ViewBag.Agents = db.Agents.Where(a => a.Status == "A")
                        .Select(a => new SelectListItem
                        {
                            Value = a.PNO,
                            Text = a.Name + " (" + a.PNO + ")",
                            Selected = a.PNO == updatedGroup.ManagerPno
                        }).ToList();

                    ViewBag.Programs = db.Programs.Select(p => new SelectListItem
                    {
                        Value = p.ProgramId.ToString(),
                        Text = p.Name,
                        Selected = p.ProgramId == updatedGroup.ProgramId
                    }).ToList();

                    return View("~/Views/Admin/Groups/Edit.cshtml", updatedGroup);
                }

                var group = db.Groups.Find(updatedGroup.Gid);
                if (group != null)
                {
                    group.GroupName = updatedGroup.GroupName;
                    group.ManagerPno = updatedGroup.ManagerPno;
                    group.ProgramId = updatedGroup.ProgramId;

                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Group updated successfully.";
                }

                return RedirectToAction("Index");
            }

            // Re-populate dropdowns if model state is invalid
            ViewBag.Agents = db.Agents.Where(a => a.Status == "A")
                .Select(a => new SelectListItem
                {
                    Value = a.PNO,
                    Text = a.Name + " (" + a.PNO + ")",
                    Selected = a.PNO == updatedGroup.ManagerPno
                }).ToList();

            ViewBag.Programs = db.Programs.Select(p => new SelectListItem
            {
                Value = p.ProgramId.ToString(),
                Text = p.Name,
                Selected = p.ProgramId == updatedGroup.ProgramId
            }).ToList();

            return View("~/Views/Admin/Groups/Edit.cshtml", updatedGroup);
        }

        // Delete group
        public ActionResult Delete(int id)
        {
            var group = db.Groups.Find(id);
            if (group != null)
            {
                // Check if group has members
                var memberCount = db.Database.SqlQuery<int>(
                    "SELECT COUNT(*) FROM AgentGroup WHERE gid = @p0", id).FirstOrDefault();

                if (memberCount > 0)
                {
                    TempData["ErrorMessage"] = $"Cannot delete group. It has {memberCount} member(s). Please remove all members first.";
                }
                else
                {
                    db.Groups.Remove(group);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Group deleted successfully.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Group not found.";
            }

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    // Helper class for Group Members
    public class GroupMember
    {
        public int Sno { get; set; }
        public string Pno { get; set; }
        public string Name { get; set; }
        public int Gid { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string Status { get; set; }
        public string WorkArea { get; set; }
        public DateTime? LastLogin { get; set; }
    }

    // Helper class for SQL query result
    public class AgentGroupResult
    {
        public int sno { get; set; }
        public string pno { get; set; }
        public string Name { get; set; }
        public int gid { get; set; }
    }
}