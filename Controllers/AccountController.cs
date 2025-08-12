using PIA_Admin_Dashboard.Models;
using System.Web.Mvc;
using PIA_Admin_Dashboard.Models.AuthModels;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Net.Mail;
using System.Net;

namespace PIA_Admin_Dashboard.Controllers
{
    public class AccountController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        private readonly string senderEmail = "saad.masroor8@gmail.com";
        private readonly string senderPassword = "yjhr fscs dujs zeon";
        private readonly string testRecipient = "anasdevilkhan83@gmail.com";

        // GET: Login
        public ActionResult Login()
        {
            if (Session["AgentUid"] != null)
                return RedirectToAction("Dashboard", "Admin");

            return View(new LoginModel());
        }

        // POST: Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string ipAddress = Request.UserHostAddress ?? "Unknown IP";
            string userAgent = Request.UserAgent ?? "Unknown Agent";

            void LogLoginAttempt(bool success)
            {
                db.Login_Attempts.Add(new Login_Attempts
                {
                    email = model.Email,
                    attempt_time = DateTime.Now,
                    ip_address = ipAddress,
                    user_agent = userAgent,
                    was_successful = success
                });
                db.SaveChanges();
            }

            void LogLoginActivity(string agentUid, bool success)
            {
                db.Login_Logs.Add(new Login_Logs
                {
                    agent_uid = agentUid,
                    login_method = "Password",
                    login_time = DateTime.Now,
                    ip_address = ipAddress,
                    user_agent = userAgent,
                    was_successful = success
                });
                db.SaveChanges();
            }

            var lockout = db.Account_Lockouts.FirstOrDefault(l =>
                l.lockout_type == "Login" &&
                l.ip_address == ipAddress &&
                (l.unlocks_at == null || l.unlocks_at > DateTime.Now));

            if (lockout != null)
            {
                ModelState.AddModelError("", "Your account is temporarily locked due to multiple failed login attempts.");
                LogLoginAttempt(false);
                return View(model);
            }

            var cred = db.Agent_Credentials.FirstOrDefault(c => c.email == model.Email && c.is_active);
            if (cred == null || !VerifyPassword(model.Password, cred.password_hash))
            {
                LogLoginAttempt(false);
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            LogLoginAttempt(true);
            LogLoginActivity(cred.agent_uid, true);

            var agent = db.Agents.FirstOrDefault(a => a.AgentUid == cred.agent_uid);
            if (agent == null)
            {
                ModelState.AddModelError("", "Agent details not found.");
                return View(model);
            }

            Session["AgentUid"] = agent.AgentUid;
            Session["AgentEmail"] = agent.Email;
            Session["RoleId"] = agent.RoleId;
            Session["AgentName"] = agent.Name;

            TempData["Message"] = "Login Successful!";
            return RedirectToAction("Dashboard", "Admin");
        }

        // GET: GetOtp
        public ActionResult GetOtp()
        {
            TempData.Keep("Message");
            ModelState.Clear();
            return View(new OtpModel());
        }

        // POST: GetOtp
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GetOtp(OtpModel model)
        {
            string ipAddress = Request.UserHostAddress ?? "Unknown IP";

            if (!string.IsNullOrEmpty(model.EmployeeId) && string.IsNullOrEmpty(model.ConfirmEmail))
            {
                var agent = db.Agents.FirstOrDefault(a => a.PNO == model.EmployeeId);
                if (agent == null)
                {
                    ModelState.AddModelError("EmployeeId", "No agent found with this Employee ID.");
                    return View(model);
                }

                ViewBag.MaskedEmail = MaskEmail(agent.Email);
                return View(model);
            }

            if (!ModelState.IsValid)
                return View(model);

            var agentFinal = db.Agents.FirstOrDefault(a => a.PNO == model.EmployeeId);
            if (agentFinal == null || !string.Equals(agentFinal.Email?.Trim(), model.ConfirmEmail?.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.MaskedEmail = MaskEmail(agentFinal?.Email ?? "");
                ModelState.AddModelError("ConfirmEmail", "Email does not match.");
                return View(model);
            }

            var existingLock = db.Account_Lockouts.FirstOrDefault(l =>
                l.lockout_type == "OTP" &&
                l.ip_address == ipAddress &&
                (l.unlocks_at == null || l.unlocks_at > DateTime.Now));

            if (existingLock != null)
            {
                TempData["Message"] = "🚫 You've requested OTPs too frequently. Please wait and try again later.";
                return RedirectToAction("GetOtp");
            }

            DateTime tenMinutesAgo = DateTime.Now.AddMinutes(-10);
            int recentOtps = db.OTP_Verifications.Count(o => o.ip_address == ipAddress && o.created_at > tenMinutesAgo);

            if (recentOtps >= 3)
            {
                db.Account_Lockouts.Add(new Account_Lockouts
                {
                    agent_uid = agentFinal.AgentUid,
                    lockout_type = "OTP",
                    reason = "Too many OTP requests",
                    locked_at = DateTime.Now,
                    unlocks_at = DateTime.Now.AddMinutes(1),
                    ip_address = ipAddress
                });

                db.SaveChanges();
                TempData["Message"] = "🚫 Too many OTP requests. You've been locked out for 10 minutes.";
                return RedirectToAction("GetOtp");
            }

            string generatedOtp = new Random().Next(100000, 999999).ToString();
            DateTime expiry = DateTime.Now.AddMinutes(5);

            db.OTP_Verifications.Add(new OTP_Verifications
            {
                agent_uid = agentFinal.AgentUid,
                otp_code = generatedOtp,
                is_used = false,
                created_at = DateTime.Now,
                expires_at = expiry,
                purpose = "reset",
                sent_to_email = agentFinal.Email,
                ip_address = ipAddress,
                attempt_source = "forget_password"
            });

            db.SaveChanges();

            Session["otp_sent"] = generatedOtp;
            Session["uid_for_reset"] = agentFinal.AgentUid;
            Session["otp_sent_time"] = DateTime.Now;
            Session["otp_expiry"] = expiry;

            SendTestEmail(testRecipient, generatedOtp);

            TempData["Message"] = $"✅ OTP sent to {agentFinal.Email}";
            return RedirectToAction("GetOtp");
        }

        [HttpPost]
        public ActionResult ClearOtpPopupState()
        {
            Session["otp_sent"] = null;
            return new HttpStatusCodeResult(200);
        }

        public ActionResult Dashboard()
        {
            return View();
        }

        [HttpGet]
        public ActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (Session["otp_verified"] == null || Session["uid_for_reset"] == null)
            {
                TempData["Message"] = "Unauthorized access to reset page.";
                return RedirectToAction("Login");
            }

            string uid = Session["uid_for_reset"].ToString();
            var cred = db.Agent_Credentials.FirstOrDefault(c => c.agent_uid == uid);

            if (cred != null)
            {
                cred.password_hash = HashPassword(model.NewPassword);
            }
            else
            {
                var agent = db.Agents.FirstOrDefault(a => a.AgentUid == uid);
                if (agent == null)
                {
                    ModelState.AddModelError("", "Agent not found.");
                    return View(model);
                }

                db.Agent_Credentials.Add(new Agent_Credentials
                {
                    agent_uid = agent.AgentUid,
                    email = agent.Email,
                    password_hash = HashPassword(model.NewPassword),
                    is_active = true
                });
            }

            db.SaveChanges();

            Session.Remove("uid_for_reset");
            Session.Remove("otp_verified");

            TempData["Message"] = "Password set successfully. You can now login.";
            return RedirectToAction("Login");
        }

        [HttpPost]
        public ActionResult ConfirmOtp(string otp)
        {
            string uid = Session["uid_for_reset"]?.ToString();
            if (string.IsNullOrEmpty(uid))
            {
                TempData["Message"] = "Session expired. Please try again.";
                return RedirectToAction("GetOtp");
            }

            var record = db.OTP_Verifications
                .Where(o => o.agent_uid == uid && o.purpose == "reset" && !o.is_used)
                .OrderByDescending(o => o.created_at)
                .FirstOrDefault();

            if (record == null || record.expires_at < DateTime.Now || record.otp_code != otp)
            {
                TempData["Message"] = "Invalid or expired OTP.";
                return RedirectToAction("GetOtp");
            }

            record.is_used = true;
            db.SaveChanges();

            Session["otp_verified"] = true;
            TempData["Message"] = "OTP verified. Please set your new password.";
            return RedirectToAction("ResetPassword");
        }

        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            Response.Cookies.Clear();
            return RedirectToAction("Login");
        }

        private bool VerifyPassword(string inputPassword, string storedHash)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(inputPassword);
                byte[] hashBytes = sha.ComputeHash(inputBytes);
                string inputHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                return inputHash == storedHash;
            }
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hash = sha.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }

            // TODO: Replace with stronger password hashing like PBKDF2 or bcrypt.
        }

        private string MaskEmail(string email)
        {
            var atIndex = email.IndexOf("@");
            if (atIndex <= 1) return email;

            string namePart = email.Substring(0, atIndex);
            string domain = email.Substring(atIndex);

            if (namePart.Length <= 2)
                return namePart[0] + "*" + domain;

            string masked = namePart[0] + new string('*', namePart.Length - 2) + namePart[namePart.Length - 1];
            return masked + domain;
        }

        private void SendTestEmail(string toEmail, string otp)
        {
            try
            {
                var mail = new MailMessage
                {
                    From = new MailAddress(senderEmail),
                    Subject = "Your PIA SRM OTP Code",
                    Body = $"Your OTP code is: {otp}",
                    IsBodyHtml = false
                };
                mail.To.Add(toEmail);

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(senderEmail, senderPassword),
                    DeliveryMethod = SmtpDeliveryMethod.Network
                };

                smtp.Send(mail);
                TempData["Message"] = $"✅ OTP email successfully sent to: {toEmail}";
            }
            catch (Exception ex)
            {
                TempData["Message"] = "❌ OTP could not be sent. Please try again or contact support.";
                System.Diagnostics.Debug.WriteLine("EMAIL ERROR: " + ex.Message);
            }

            TempData.Keep("Message");
        }
    }
}
