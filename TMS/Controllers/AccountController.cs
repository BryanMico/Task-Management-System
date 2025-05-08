using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using TMS.Models;
using System.Data.Entity;
using System.Web;

namespace TMS.Controllers
{
    public class AccountController : Controller
    {
        private readonly TMS_DB _db = new TMS_DB();

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Please provide all required fields.";
                    return View(model);
                }

                var user = _db.Users.Include(u => u.Role)
                                    .FirstOrDefault(u => u.Email.ToLower() == model.Email.ToLower() && u.IsActive);

                if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                {
                    TempData["ErrorMessage"] = "Invalid email or password.";
                    return View(model);
                }

                if (user.Role == null)
                {
                    TempData["ErrorMessage"] = "User role is not assigned. Contact support.";
                    return View(model);
                }

                // Set session variables
                Session["UserID"] = user.Id;
                Session["UserRole"] = user.Role.Name;
                Session["FullName"] = user.FullName;
                Session["UserEmail"] = user.Email;

                // Authenticate user
                FormsAuthentication.SetAuthCookie(user.Email, false);

                // Success message
                TempData["SuccessMessage"] = "Login successful! Welcome, " + user.FullName + ".";

                // Redirect based on role
                switch (user.Role.Name)
                {
                    case "Admin":
                        return RedirectToAction("Dashboard_Admin", "Dashboard");
                    case "Client":
                        return RedirectToAction("Dashboard_Client", "Dashboard");
                    default:
                        TempData["ErrorMessage"] = "Invalid user role.";
                        return RedirectToAction("Login");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}\nStackTrace: {ex.StackTrace}");
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again.";
                return View(model);
            }
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            // Sign out the user
            FormsAuthentication.SignOut();

            // Clear session data
            Session.Clear();
            Session.Abandon();

            // Remove authentication cookie
            if (Request.Cookies[".ASPXAUTH"] != null)
            {
                var cookie = new HttpCookie(".ASPXAUTH")
                {
                    Expires = DateTime.Now.AddDays(-1),
                    HttpOnly = true
                };
                Response.Cookies.Add(cookie);
            }

            // Prevent browser from caching the page
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
            Response.Cache.SetNoStore();

            return RedirectToAction("Login");
        }

    }
}