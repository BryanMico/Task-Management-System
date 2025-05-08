using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TMS.Filters;
using TMS.Models;
using System.Data.Entity;

namespace TMS.Controllers
{
    public class DashboardController : Controller
    {

        private readonly TMS_DB _db = new TMS_DB();

        [AuthorizeRole("Admin")]
        public ActionResult Dashboard_Admin()
        {
            try
            {
                string userEmail = User.Identity.Name;
                if (string.IsNullOrEmpty(userEmail))
                {
                    TempData["ErrorMessage"] = "Session expired. Please log in again.";
                    return RedirectToAction("Login", "Account");
                }

                var user = _db.Users.FirstOrDefault(u => u.Email == userEmail);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found. Please contact support.";
                    return RedirectToAction("Logout", "Account");
                }

                string fullName = user.FullName ?? "User";

                var totalTasks = _db.Tasks?.Count() ?? 0;
                var pendingTasks = _db.Tasks?.Count(t => t.Status == "Pending") ?? 0;
                var inProgressTasks = _db.Tasks?.Count(t => t.Status == "In Progress") ?? 0;
                var completedTasks = _db.Tasks?.Count(t => t.Status == "Completed") ?? 0;

                var latestTasks = _db.Tasks?.OrderByDescending(t => t.CreatedDate).Take(3).Select(t => new TaskViewModel
                {
                    Id = t.Id,
                    Title = t.Title,
                    DueDate = t.DueDate,
                    Status = t.Status,
                    Priority = t.Priority,
                    CreatedBy = t.CreatedBy != null ? t.CreatedBy.FullName : "Unknown",
                    AssignedTo = t.AssignedTo != null ? t.AssignedTo.FullName : "Unassigned",
                    StartDate = t.StartDate
                }).ToList() ?? new List<TaskViewModel>();

                var viewModel = new DashboardViewModel
                {
                    UserFullName = fullName,
                    UserEmail = userEmail,
                    TotalTasks = totalTasks,
                    PendingTasks = pendingTasks,
                    InProgressTasks = inProgressTasks,
                    CompletedTasks = completedTasks,
                    LatestTasks = latestTasks
                };

                ViewBag.UserFullName = fullName;
                ViewBag.UserEmail = userEmail;

                return View(viewModel);
            }
            catch
            {
                TempData["ErrorMessage"] = "An error occurred while loading the dashboard. Please try again.";
                return RedirectToAction("Login", "Account");
            }
        }


        [AuthorizeRole("Client")]
        public ActionResult Dashboard_Client()
        {
            try
            {
                string userEmail = User.Identity.Name;
                if (string.IsNullOrEmpty(userEmail))
                {
                    TempData["ErrorMessage"] = "Session expired. Please log in again.";
                    return RedirectToAction("Login", "Account");
                }

                var user = _db.Users.FirstOrDefault(u => u.Email == userEmail);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found. Please contact support.";
                    return RedirectToAction("Logout", "Account");
                }

                string fullName = user.FullName ?? "User";
                bool isNewUser = user.IsNewUser;

                var clientTasks = _db.Tasks?.Where(t => t.AssignedTo != null && t.AssignedTo.Email == userEmail)
                    .Select(t => new TaskViewModel
                    {
                        Id = t.Id,
                        Title = t.Title,
                        DueDate = t.DueDate,
                        Status = t.Status,
                        Priority = t.Priority,
                        CreatedBy = t.CreatedBy != null ? t.CreatedBy.FullName : "Unknown",
                        AssignedTo = t.AssignedTo != null ? t.AssignedTo.FullName : "Unassigned",
                        StartDate = t.StartDate
                    }).ToList() ?? new List<TaskViewModel>();

                var totalTasks = clientTasks.Count;
                var pendingTasks = clientTasks.Count(t => t.Status == "Pending");
                var inProgressTasks = clientTasks.Count(t => t.Status == "In Progress");
                var completedTasks = clientTasks.Count(t => t.Status == "Completed");

                var viewModel = new DashboardViewModel
                {
                    UserFullName = fullName,
                    UserEmail = userEmail,
                    TotalTasks = totalTasks,
                    PendingTasks = pendingTasks,
                    InProgressTasks = inProgressTasks,
                    CompletedTasks = completedTasks,
                    LatestTasks = clientTasks.Take(3).ToList(),
                    IsNewUser = isNewUser
                };

                ViewBag.UserFullName = fullName;
                ViewBag.UserEmail = userEmail;

                return View(viewModel);
            }
            catch
            {
                TempData["ErrorMessage"] = "An error occurred while loading the dashboard. Please try again.";
                return RedirectToAction("Login", "Account");
            }
        }

    }
}