using System.Web.Mvc;
using PIA_SRM_MVC.Models;

namespace PIA_SRM_MVC.Controllers
{
    public class AccountController : Controller
    {
        // GET: Login
        public ActionResult Login()
        {
            // Always return an empty model to clear old data
            return View(new LoginModel());
        }


        // POST: Login
        [HttpPost]
        public ActionResult Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Username == "admin" && model.Password == "pia123")
                {
                    // TempData can be replaced with session or auth cookie
                    TempData["Message"] = "Login Successful!";
                    return RedirectToAction("Dashboard", "Admin");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid credentials");
                }
            }
            return View(model);
        }
        // GET: GetOtp
        public ActionResult GetOtp()
        {
            return View();
        }

        // POST: GetOtp
        [HttpPost]
        public ActionResult GetOtp(OtpModel model)
        {
            if (ModelState.IsValid)
            {
                // Simulate sending OTP
                TempData["Message"] = "OTP sent to registered contact!";
                return RedirectToAction("Login");
            }
            return View(model);
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
            if (ModelState.IsValid)
            {
                // Here you would update the password in the database
                TempData["Message"] = "Password reset successfully!";
                return RedirectToAction("Login");
            }

            return View(model);
        }
        [HttpPost]
        public ActionResult ConfirmOtp(string otp)
        {
            // ✅ Replace this logic with your OTP verification
            if (otp == "123456") // Sample logic
            {
                TempData["Message"] = "OTP verified. Please set your password.";
                return RedirectToAction("ResetPassword");
            }

            TempData["Message"] = "Invalid OTP. Try again.";
            return RedirectToAction("GetOtp");
        }
    }
}
