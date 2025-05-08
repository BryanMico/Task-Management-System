using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TMS.Models;
using PagedList; 
using PagedList.Mvc;
using TMS.Filters;

namespace TMS.Controllers
{
    public class UserController : Controller
    {
        private readonly TMS_DB _db = new TMS_DB();

        private void SetLoggedInUserDetails()
        {
            string userEmail = User.Identity.Name;
            string fullName = "User";

            if (!string.IsNullOrEmpty(userEmail))
            {
                var currentUser = _db.Users.FirstOrDefault(u => u.Email == userEmail);
                if (currentUser != null)
                {
                    fullName = currentUser.FullName;
                }
            }
            ViewBag.UserFullName = fullName;
            ViewBag.UserEmail = userEmail;
        }

        [AuthorizeRole("Admin")]
        public ActionResult Users_List(string searchQueryUsers, string roleFilter, int? page)
        {
            try
            {
                SetLoggedInUserDetails();
                int pageSize = 6;
                int pageNumber = (page ?? 1);

                var users = _db.Users
                    .Select(u => new UserViewModel
                    {
                        Id = u.Id,
                        FullName = u.FullName,
                        Email = u.Email,
                        RoleName = u.Role.Name,
                        IsActive = u.IsActive
                    });

                if (!users.Any())
                {
                    TempData["ErrorMessage"] = "No users found.";
                    return View(new List<UserViewModel>().ToPagedList(pageNumber, pageSize));
                }

                if (!string.IsNullOrEmpty(roleFilter))
                {
                    users = users.Where(u => u.RoleName == roleFilter);
                }

                if (!string.IsNullOrEmpty(searchQueryUsers))
                {
                    users = users.Where(u => u.FullName.Contains(searchQueryUsers) || u.Email.Contains(searchQueryUsers));
                }

                ViewBag.SearchQuery = searchQueryUsers;
                ViewBag.RoleFilter = roleFilter;

                return View(users.OrderBy(u => u.FullName).ToPagedList(pageNumber, pageSize));
            }
            catch
            {
                TempData["ErrorMessage"] = "An error occurred while retrieving the user list. Please try again.";
                return RedirectToAction("Dashboard_Admin", "Dashboard");

            }
        }

        public ActionResult GetUsersList(string searchQuery)
        {
            try
            {
                var users = _db.Users
                    .Where(u => string.IsNullOrEmpty(searchQuery) ||
                                u.FullName.Contains(searchQuery) ||
                                u.Email.Contains(searchQuery))
                    .Select(u => new UserViewModel
                    {
                        Id = u.Id,
                        FullName = u.FullName,
                        Email = u.Email,
                        RoleName = u.Role != null ? u.Role.Name : "No Role Assigned"
                    }).ToList();

                if (!users.Any())
                {
                    TempData["ErrorMessage"] = "No users found.";
                    return PartialView("~/Views/Dashboard/_UsersTab.cshtml", new List<UserViewModel>());
                }

                ViewBag.TotalUsers = users.Count;
                ViewBag.SearchQuery = searchQuery;
                return PartialView("~/Views/Dashboard/_UsersTab.cshtml", users);
            }
            catch
            {
                TempData["ErrorMessage"] = "An error occurred while retrieving the user list. Please try again.";
                return RedirectToAction("Dashboard_Admin", "Dashboard");
            }
        }


        [AuthorizeRole("Admin")]
        public ActionResult User_Manage(int? id)
        {
            try
            {
                SetLoggedInUserDetails();

                var user = id.HasValue ? _db.Users.Find(id) : new User();

                if (user == null && id.HasValue)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction("Users_List", "User");
                }

                ViewBag.Roles = new SelectList(_db.Roles.ToList(), "Id", "Name", user.RoleId);

                return View(user);
            }
            catch
            {
                TempData["ErrorMessage"] = "An error occurred while loading the user details.";
                return RedirectToAction("Dashboard_Admin", "Dashboard");

            }
        }

        [AuthorizeRole("Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult User_Manage(User model)
        {
            try
            {
                if (model.Id != 0)
                {
                    ModelState.Remove("PasswordHash");
                }

                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Please check your inputs and try again.";
                    ViewBag.Roles = new SelectList(_db.Roles.ToList(), "Id", "Name", model.RoleId);
                    return View(model);
                }

                if (model.Id == 0)
                {
                    model.IsNewUser = true;
                    model.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.PasswordHash);
                    _db.Users.Add(model);
                    TempData["SuccessMessage"] = "User " + model.FullName + " created successfully.";
                }
                else
                {
                    var existingUser = _db.Users.Find(model.Id);
                    if (existingUser == null)
                    {
                        TempData["ErrorMessage"] = "User not found.";
                        return RedirectToAction("Users_List", "User");
                    }

                    existingUser.FullName = model.FullName;
                    existingUser.Email = model.Email;
                    existingUser.RoleId = model.RoleId;
                    existingUser.IsActive = model.IsActive;
                    existingUser.IsNewUser = model.IsNewUser;

                    if (!string.IsNullOrEmpty(model.PasswordHash))
                    {
                        existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.PasswordHash);
                    }

                    _db.Entry(existingUser).State = EntityState.Modified;
                    TempData["SuccessMessage"] = "User " + existingUser.FullName + " updated successfully.";
                }

                _db.SaveChanges();
                return RedirectToAction("Users_List", "User");
            }
            catch
            {
                TempData["ErrorMessage"] = "An error occurred while saving the user. Please try again.";
                return View(model);
            }
        }

        [AuthorizeRole("Admin")]
        public ActionResult User_Delete(int id)
        {
            try
            {
                var user = _db.Users.Find(id);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction("Users_List", "User");
                }

                return PartialView("_User_Remove", user);
            }
            catch {
                TempData["ErrorMessage"] = "An error occurred while trying to delete the user.";
                return RedirectToAction("Users_List", "User");
            }
        }

        [AuthorizeRole("Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult User_DeleteConfirmed(int id)
        {
            try
            {
                var user = _db.Users.Find(id);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction("Users_List", "User");
                }

                var tasks = _db.Tasks.Where(t => t.AssignedToId == user.Id).ToList();
                if (tasks.Any())
                {
                    TempData["ErrorMessage"] = "Cannot delete user. They are assigned to one or more tasks.";
                    return RedirectToAction("Users_List");
                }

                _db.Users.Remove(user);
                _db.SaveChanges();

                TempData["SuccessMessage"] = "User " + user.FullName + " removed successfully.";
                return RedirectToAction("Users_List", "User");
            }
            catch
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the user.";
                return RedirectToAction("Users_List", "User");
            }
        }

        [AuthorizeRole("Admin")]
        public ActionResult User_View(int? page, int? userId, string searchQueryTask, string statusFilter)
        {
            try
            {
                SetLoggedInUserDetails();
                var users = _db.Users.ToList();
                ViewBag.Users = users;

                if (userId == null)
                {
                    ViewBag.SelectedUser = "Select a User";
                    return View(new PagedList<TaskViewModel>(new List<TaskViewModel>(), 1, 10)); // Empty list
                }

                var selectedUser = _db.Users.Find(userId);
                if (selectedUser == null)
                {
                    ViewBag.SelectedUser = "User Not Found";
                    return View(new PagedList<TaskViewModel>(new List<TaskViewModel>(), 1, 10)); // Empty list
                }

                ViewBag.SelectedUser = selectedUser.FullName;
                ViewBag.SelectedUserId = userId;

                var tasksQuery = _db.Tasks.Where(t => t.AssignedToId == userId);



                var tasks = tasksQuery
                    .Select(t => new TaskViewModel
                    {
                        Title = t.Title,
                        TaskType = t.TaskType.ToString(), // Enum converted to string
                        CreatedBy = t.CreatedBy != null ? t.CreatedBy.FullName : "Unknown", // Avoid null reference
                        StartDate = t.StartDate,
                        DueDate = t.DueDate,
                        Priority = t.Priority,
                        Status = t.Status
                    })
                    .ToList();


                int pageSize = 5;
                int pageNumber = (page ?? 1);
                return View(tasks.ToPagedList(pageNumber, pageSize));
            }
            catch {
                TempData["ErrorMessage"] = "An error occurred while loading the user details. Please try again.";
                return RedirectToAction("Users_List", "User");
            }
        }

        [AuthorizeRole("Client")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                             
                TempData["ErrorMessage"] = "Please correct the errors and try again.";
                TempData["ShowChangePasswordModal"] = true; 
                return RedirectToAction("Dashboard_Client", "Dashboard");
            }

            var user = _db.Users.FirstOrDefault(u => u.Id == model.UserId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                TempData["ShowChangePasswordModal"] = true;
                return RedirectToAction("Dashboard_Client", "Dashboard");
            }

            // ✅ Update password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            user.IsNewUser = false;
            _db.SaveChanges();

            TempData["SuccessMessage"] = "Password updated successfully!";

            return RedirectToAction("Dashboard_Client", "Dashboard");
        }



    }

}