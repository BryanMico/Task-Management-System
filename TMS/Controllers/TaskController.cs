using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TMS.Filters;
using TMS.Models;

namespace TMS.Controllers
{
    public class TaskController : Controller
    {
        private readonly TMS_DB _db = new TMS_DB(); // Use your TMS_DB context

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
        public ActionResult Tasks_List(string searchQueryTask, string statusFilter, int? page)
        {
            try
            {
                SetLoggedInUserDetails();

                int pageSize = 6;
                int pageNumber = (page ?? 1);

                var tasks = _db.Tasks
                    .Include(t => t.CreatedBy)
                    .Include(t => t.AssignedTo)
                    .Include(t => t.Project)
                    .Select(t => new TaskViewModel
                    {
                        Id = t.Id,
                        Title = t.Title,
                        TaskType = t.TaskType.ToString(),
                        CreatedBy = t.CreatedBy != null ? t.CreatedBy.FullName : "Unknown",
                        AssignedTo = t.AssignedTo != null ? t.AssignedTo.FullName : "Unassigned",
                        StartDate = t.StartDate,
                        DueDate = t.DueDate,
                        Priority = t.Priority,
                        Status = t.Status
                    });

                if (!tasks.Any())
                {
                    TempData["ErrorMessage"] = "No tasks found.";
                    return View(new List<TaskViewModel>().ToPagedList(pageNumber, pageSize));
                }

                if (!string.IsNullOrEmpty(statusFilter))
                {
                    tasks = tasks.Where(t => t.Status == statusFilter);
                }

                if (!string.IsNullOrEmpty(searchQueryTask))
                {
                    tasks = tasks.Where(t => t.Title.Contains(searchQueryTask) || t.AssignedTo.Contains(searchQueryTask));
                }

                ViewBag.SearchQuery = searchQueryTask;
                ViewBag.StatusFilter = statusFilter;

                return View(tasks.OrderBy(t => t.StartDate).ToPagedList(pageNumber, pageSize));
            }
            catch
            {
                TempData["ErrorMessage"] = "An error occurred while retrieving the task list. Please try again.";
                return RedirectToAction("Dashboard_Admin", "Dashboard");
            }
        }

        [AuthorizeRole("Admin")]
        public ActionResult Task_Manage(int? id)
        {
            try
            {
                SetLoggedInUserDetails();
                var task = id.HasValue ? _db.Tasks.Find(id) : new Task();
                if (task == null && id.HasValue)
                {
                    TempData["ErrorMessage"] = "Task not found.";
                    return RedirectToAction("Tasks_List", "Task");
                }
                ViewBag.TaskTypes = new SelectList(Enum.GetValues(typeof(TaskType)));
                ViewBag.Users = _db.Users.Select(u => new { u.Id, FullName = u.FullName }).ToList();
                ViewBag.Projects = _db.Projects.Select(p => new { p.Id, p.Name }).ToList();

                return View(task);
            }
            catch
            {
                TempData["ErrorMessage"] = "An error occurred while loading the task details.";
                return RedirectToAction("Tasks_List", "Task");
            }
        }

        [AuthorizeRole("Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Task_Manage(Task model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.TaskTypes = new SelectList(Enum.GetValues(typeof(TaskType)));
                    ViewBag.Users = _db.Users.Select(u => new { u.Id, FullName = u.FullName }).ToList();
                    ViewBag.Projects = _db.Projects.Select(p => new { p.Id, p.Name }).ToList();
                    TempData["ErrorMessage"] = "Please check your inputs and try again.";
                    return View(model);
                }

                string userEmail = User.Identity.Name;
                var currentUser = _db.Users.FirstOrDefault(u => u.Email == userEmail);

                if (model.Id == 0)
                {
                    model.CreatedById = currentUser.Id;
                    model.CreatedDate = DateTime.UtcNow;
                    _db.Tasks.Add(model);
                    TempData["SuccessMessage"] = "Task created successfully.";

                }
                else
                {
                    var existingTask = _db.Tasks.Find(model.Id);
                    if (existingTask == null)
                    {
                        return HttpNotFound();
                    }

                    existingTask.Title = model.Title;
                    existingTask.Description = model.Description;
                    existingTask.Status = model.Status;
                    existingTask.StartDate = model.StartDate;
                    existingTask.DueDate = model.DueDate;
                    existingTask.Priority = model.Priority;
                    existingTask.TaskType = model.TaskType;
                    existingTask.IsCompleted = model.IsCompleted;
                    existingTask.UpdatedDate = DateTime.UtcNow;

                    existingTask.AssignedToId = model.AssignedToId;
                    existingTask.ProjectId = model.ProjectId;

                    TempData["SuccessMessage"] = "Task updated successfully.";
                    _db.Entry(existingTask).State = EntityState.Modified;
                }

                _db.SaveChanges();
                return RedirectToAction("Tasks_List", "Task");

            }
            catch
            {
                ViewBag.TaskTypes = new SelectList(Enum.GetValues(typeof(TaskType)));
                ViewBag.Users = _db.Users.Select(u => new { u.Id, FullName = u.FullName }).ToList();
                ViewBag.Projects = _db.Projects.Select(p => new { p.Id, p.Name }).ToList();
                TempData["ErrorMessage"] = "An error occurred while saving the task. Please try again.";
                return View(model);
            }
        }

        [AuthorizeRole("Admin")]
        public ActionResult Task_Delete(int id)
        {
            try
            {
                var task = _db.Tasks.Find(id);
                if (task == null)
                {
                    TempData["ErrorMessage"] = "Task not found.";
                    return RedirectToAction("Tasks_List", "Task");
                }

                return PartialView("_Task_Remove", task);
            }
            catch
            {
                TempData["ErrorMessage"] = "An error occurred while trying to delete the task.";
                return RedirectToAction("Tasks_List", "Task");

            }
        }

        [AuthorizeRole("Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Task_DeleteConfirmed(int id)
        {
            try
            {
                var task = _db.Tasks.Find(id);
                if (task == null)
                {
                    TempData["ErrorMessage"] = "Task not found.";
                    return RedirectToAction("Tasks_List", "Task");
                }

                _db.Tasks.Remove(task);
                _db.SaveChanges();

                TempData["SuccessMessage"] = "Task deleted successfully.";
                return RedirectToAction("Tasks_List", "Task");
            }
            catch
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the task.";
                return RedirectToAction("Tasks_List", "Task");

            }
        }

        [AuthorizeRole("Admin")]
        public ActionResult Task_View(int id)
        {
            SetLoggedInUserDetails();

            // Get the task from the database
            var task = _db.Tasks
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedTo)
                .Where(t => t.Id == id)
                .Select(t => new TaskViewModel
                {
                    Id = t.Id,
                    Title = t.Title,
                    TaskType = t.TaskType.ToString(),
                    CreatedBy = t.CreatedBy != null ? t.CreatedBy.FullName : "Unknown",
                    AssignedTo = t.AssignedTo != null ? t.AssignedTo.FullName : "Unassigned",
                    StartDate = t.StartDate,
                    DueDate = t.DueDate,
                    Priority = t.Priority,
                    Status = t.Status,
                    Description = t.Description,
                    CreatedDate = t.CreatedDate,
                    UpdatedDate = (DateTime)t.UpdatedDate,
                    TaskNotes = t.TaskNotes, // Pass the note to the view
                    CompletedBy = t.AssignedTo.FullName, // Populate the name of the person who completed the task
                    CompletedDate = (DateTime)t.UpdatedDate,
                    TransferredBy = t.TransferredBy,
                    TransferredTo = t.TransferredTo,
                    TransferredDate = t.TransferredDate,
                }).FirstOrDefault();

            if (task == null)
            {
                return HttpNotFound();
            }

            return View(task);
        }

        [AuthorizeRole("Admin")]
        public ActionResult Tasks_Assigned(string searchQueryTask, string statusFilter, int? page)
        {
            string userEmail = User.Identity.Name;
            string fullName = "User";

            // Get the current logged-in user details
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

            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _db.Users.FirstOrDefault(u => u.Email == userEmail);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Pagination settings
            int pageSize = 6;
            int pageNumber = (page ?? 1);

            // Start querying tasks
            var tasksQuery = _db.Tasks
                .Where(t => t.AssignedToId == user.Id)
                .OrderByDescending(t => t.CreatedDate)
                .Select(t => new TaskViewModel
                {
                    Id = t.Id,
                    Title = t.Title,
                    TaskType = t.TaskType.ToString(),
                    CreatedBy = t.CreatedBy.FullName,
                    AssignedTo = t.AssignedTo.FullName,
                    Status = t.Status,
                    Priority = t.Priority,
                    StartDate = t.StartDate,
                    DueDate = t.DueDate
                });

            // Apply filters before pagination
            if (!string.IsNullOrEmpty(statusFilter))
            {
                tasksQuery = tasksQuery.Where(t => t.Status == statusFilter);
            }

            if (!string.IsNullOrEmpty(searchQueryTask))
            {
                tasksQuery = tasksQuery.Where(t => t.Title.Contains(searchQueryTask) || t.AssignedTo.Contains(searchQueryTask));
            }

            // Apply pagination to filtered tasks
            var tasks = tasksQuery.ToPagedList(pageNumber, pageSize);

            // Pass filtered data to the view
            ViewBag.SearchQuery = searchQueryTask;
            ViewBag.StatusFilter = statusFilter;

            return View(tasks);
        }

        [AuthorizeRole("Client")]
        public ActionResult Task_View_Client(int id)
        {
            SetLoggedInUserDetails();
            string userEmail = User.Identity.Name;

            // Get the task from the database
            var task = _db.Tasks
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedTo)
                .Where(t => t.Id == id)
                .Select(t => new TaskViewModel
                {
                    Id = t.Id,
                    Title = t.Title,
                    TaskType = t.TaskType.ToString(),
                    CreatedBy = t.CreatedBy != null ? t.CreatedBy.FullName : "Unknown",
                    AssignedTo = t.AssignedTo != null ? t.AssignedTo.FullName : "Unassigned",
                    StartDate = t.StartDate,
                    DueDate = t.DueDate,
                    Priority = t.Priority,
                    Status = t.Status,
                    Description = t.Description,
                    CreatedDate = t.CreatedDate,
                    UpdatedDate = (DateTime)t.UpdatedDate,
                    TaskNotes = t.TaskNotes, // Pass the note to the view
                    CompletedBy = t.AssignedTo.FullName, // Populate the name of the person who completed the task
                    CompletedDate = (DateTime)t.UpdatedDate,
                    TransferredBy = t.TransferredBy,
                    TransferredTo = t.TransferredTo,
                    TransferredDate = t.TransferredDate,
                }).FirstOrDefault();

            if (task == null)
            {
                return HttpNotFound();
            }
            var currentUser = _db.Users.FirstOrDefault(u => u.Email == userEmail);

            // Get list of other clients (excluding the current user)
            if (currentUser != null)
            {
                var clients = _db.Users
                    .Where(u => u.Role.Name == "Client" && u.Id != currentUser.Id) // Filter clients except the logged-in user
                    .OrderBy(u => u.FullName)
                    .ToList();

                ViewBag.UsersList = clients;
            }
            return View(task);
        }

        [AuthorizeRole("Admin")]
        public ActionResult Task_View_Admin(int id)
        {
            SetLoggedInUserDetails();
            string userEmail = User.Identity.Name;

            // Get the task from the database
            var task = _db.Tasks
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedTo)
                .Where(t => t.Id == id)
                .Select(t => new TaskViewModel
                {
                    Id = t.Id,
                    Title = t.Title,
                    TaskType = t.TaskType.ToString(),
                    CreatedBy = t.CreatedBy != null ? t.CreatedBy.FullName : "Unknown",
                    AssignedTo = t.AssignedTo != null ? t.AssignedTo.FullName : "Unassigned",
                    StartDate = t.StartDate,
                    DueDate = t.DueDate,
                    Priority = t.Priority,
                    Status = t.Status,
                    Description = t.Description,
                    CreatedDate = t.CreatedDate,
                    UpdatedDate = (DateTime)t.UpdatedDate,
                    TaskNotes = t.TaskNotes, // Pass the note to the view
                    CompletedBy = t.AssignedTo.FullName, // Populate the name of the person who completed the task
                    CompletedDate = (DateTime)t.UpdatedDate,
                    TransferredBy = t.TransferredBy,
                    TransferredTo = t.TransferredTo,
                    TransferredDate = t.TransferredDate,
                }).FirstOrDefault();

            if (task == null)
            {
                return HttpNotFound();
            }
            var currentUser = _db.Users.FirstOrDefault(u => u.Email == userEmail);

            // Get list of other clients (excluding the current user)
            if (currentUser != null)
            {
                var clients = _db.Users
                    .Where(u => u.Role.Name == "Client" && u.Id != currentUser.Id) // Filter clients except the logged-in user
                    .OrderBy(u => u.FullName)
                    .ToList();

                ViewBag.UsersList = clients;
            }
            return View(task);
        }

        [AuthorizeRole("Client")]
        public ActionResult Tasks_Assigned_Client(string searchQueryTask, string statusFilter, int? page)
        {
            string userEmail = User.Identity.Name;
            string fullName = "User";

            // Get the current logged-in user details
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

            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _db.Users.FirstOrDefault(u => u.Email == userEmail);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Pagination settings
            int pageSize = 6;
            int pageNumber = (page ?? 1);

            // Start querying tasks
            var tasksQuery = _db.Tasks
                .Where(t => t.AssignedToId == user.Id)
                .OrderByDescending(t => t.CreatedDate)
                .Select(t => new TaskViewModel
                {
                    Id = t.Id,
                    Title = t.Title,
                    TaskType = t.TaskType.ToString(),
                    CreatedBy = t.CreatedBy.FullName,
                    AssignedTo = t.AssignedTo.FullName,
                    Status = t.Status,
                    Priority = t.Priority,
                    StartDate = t.StartDate,
                    DueDate = t.DueDate
                });

            // Apply filters before pagination
            if (!string.IsNullOrEmpty(statusFilter))
            {
                tasksQuery = tasksQuery.Where(t => t.Status == statusFilter);
            }

            if (!string.IsNullOrEmpty(searchQueryTask))
            {
                tasksQuery = tasksQuery.Where(t => t.Title.Contains(searchQueryTask) || t.AssignedTo.Contains(searchQueryTask));
            }

            // Apply pagination to filtered tasks
            var tasks = tasksQuery.ToPagedList(pageNumber, pageSize);

            // Pass filtered data to the view
            ViewBag.SearchQuery = searchQueryTask;
            ViewBag.StatusFilter = statusFilter;

            return View(tasks);
        }


        [AuthorizeRole("Client")]
        public ActionResult Tasks_List_Client(string searchQueryTask, string statusFilter, int? page)
        {
            string userEmail = User.Identity.Name;
            string fullName = "User";

            // Get the current logged-in user details
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

            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _db.Users.FirstOrDefault(u => u.Email == userEmail);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Pagination settings
            int pageSize = 6;
            int pageNumber = (page ?? 1);

            // Start querying tasks that are created by the current user
            var tasksQuery = _db.Tasks
                .Where(t => t.CreatedById == user.Id)  // Filtering by CreatedById (tasks created by the user)
                .OrderByDescending(t => t.CreatedDate)
                .Select(t => new TaskViewModel
                {
                    Id = t.Id,
                    Title = t.Title,
                    TaskType = t.TaskType.ToString(),
                    CreatedBy = t.CreatedBy.FullName,
                    AssignedTo = t.AssignedTo.FullName,
                    Status = t.Status,
                    Priority = t.Priority,
                    StartDate = t.StartDate,
                    DueDate = t.DueDate
                });

            // Apply filters before pagination
            if (!string.IsNullOrEmpty(statusFilter))
            {
                tasksQuery = tasksQuery.Where(t => t.Status == statusFilter);
            }

            if (!string.IsNullOrEmpty(searchQueryTask))
            {
                tasksQuery = tasksQuery.Where(t => t.Title.Contains(searchQueryTask) || t.AssignedTo.Contains(searchQueryTask));
            }

            // Apply pagination to filtered tasks
            var tasks = tasksQuery.ToPagedList(pageNumber, pageSize);

            // Pass filtered data to the view
            ViewBag.SearchQuery = searchQueryTask;
            ViewBag.StatusFilter = statusFilter;

            return View(tasks);
        }

        [HttpPost]
        [AuthorizeRole("Client")]
        public ActionResult MarkTaskAsComplete(int id, string note)
        {
            try
            {
                string userEmail = User.Identity.Name;
                var currentUser = _db.Users.FirstOrDefault(u => u.Email == userEmail);
                var task = _db.Tasks.FirstOrDefault(t => t.Id == id);

                if (task != null && currentUser != null && task.AssignedToId == currentUser.Id)
                {
                    task.Status = "Completed";
                    task.TaskNotes = note;
                    task.UpdatedDate = DateTime.Now;

                    _db.SaveChanges();

                    TempData["SuccessMessage"] = "Task marked as complete successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "You are not authorized to complete this task or task not found.";
                }
            }
            catch
            {
                TempData["ErrorMessage"] = "An error occurred while completing the task.";
            }

            return RedirectToAction("Tasks_Assigned_Client", "Task");
        }


        [HttpPost]
        [AuthorizeRole("Admin")]
        public ActionResult MarkTaskAsComplete_Admin(int id, string note)
        {
            try
            {
                string userEmail = User.Identity.Name;
                var currentUser = _db.Users.FirstOrDefault(u => u.Email == userEmail);

                var task = _db.Tasks.FirstOrDefault(t => t.Id == id);

                if (task != null && currentUser != null && task.AssignedToId == currentUser.Id)
                {
                    task.Status = "Completed";
                    task.TaskNotes = note;
                    task.UpdatedDate = DateTime.Now;

                    _db.SaveChanges();
                    TempData["SuccessMessage"] = "Task successfully marked as complete.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Unauthorized or task not found.";
                }
            }
            catch 
            {
                TempData["ErrorMessage"] = "An error occurred while completing the task.";
            }

            return RedirectToAction("Tasks_Assigned", "Task");
        }


        [HttpPost]
        [AuthorizeRole("Client")]
        public ActionResult TransferTask(int TaskId, int TransferToUserId, string TransferNote)
        {
            try
            {
                string userEmail = User.Identity.Name;
                var currentUser = _db.Users.FirstOrDefault(u => u.Email == userEmail);

                var task = _db.Tasks.Include(t => t.AssignedTo).FirstOrDefault(t => t.Id == TaskId);
                var newUser = _db.Users.FirstOrDefault(u => u.Id == TransferToUserId);

                if (task != null && currentUser != null && newUser != null && task.AssignedToId == currentUser.Id)
                {
                    task.AssignedToId = newUser.Id;
                    task.TransferredBy = currentUser.FullName;
                    task.TransferredTo = newUser.FullName;
                    task.TransferredDate = DateTime.Now;
                    task.TaskNotes = TransferNote;
                    task.UpdatedDate = DateTime.Now;

                    _db.SaveChanges();
                    TempData["SuccessMessage"] = "Task transferred successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Transfer failed. Invalid task or user.";
                }
            }
            catch
            {
                TempData["ErrorMessage"] = "An error occurred while transferring the task.";
            }

            return RedirectToAction("Tasks_Assigned_Client", "Task");
        }


        [HttpPost]
        [AuthorizeRole("Admin")]
        public ActionResult TransferTask_Admin(int TaskId, int TransferToUserId, string TransferNote)
        {
            try
            {
                string userEmail = User.Identity.Name;
                var currentUser = _db.Users.FirstOrDefault(u => u.Email == userEmail);

                var task = _db.Tasks.Include(t => t.AssignedTo).FirstOrDefault(t => t.Id == TaskId);
                var newUser = _db.Users.FirstOrDefault(u => u.Id == TransferToUserId);

                if (task != null && currentUser != null && newUser != null && task.AssignedToId == currentUser.Id)
                {
                    task.AssignedToId = newUser.Id;
                    task.TransferredBy = currentUser.FullName;
                    task.TransferredTo = newUser.FullName;
                    task.TransferredDate = DateTime.Now;
                    task.TaskNotes = TransferNote;
                    task.UpdatedDate = DateTime.Now;

                    _db.SaveChanges();
                    TempData["SuccessMessage"] = "Task transferred successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Transfer failed. You are not authorized or data is invalid.";
                }
            }
            catch
            {
                TempData["ErrorMessage"] = "An error occurred while transferring the task.";
            }

            return RedirectToAction("Tasks_Assigned", "Task");
        }


        [AuthorizeRole("Client")]
        public ActionResult Task_Manage_Client(int? id)
        {
            try
            {
                SetLoggedInUserDetails();
                var task = id.HasValue ? _db.Tasks.Find(id) : new Task();

                ViewBag.TaskTypes = new SelectList(Enum.GetValues(typeof(TaskType)));
                ViewBag.Users = _db.Users.Select(u => new { u.Id, FullName = u.FullName }).ToList();
                ViewBag.Projects = _db.Projects.Select(p => new { p.Id, p.Name }).ToList();

                return View(task);
            }
            catch
            {
                TempData["ErrorMessage"] = "An error occurred while loading the task details.";
                return RedirectToAction("Tasks_List_Client", "Task");
            }
        }

        [AuthorizeRole("Client")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Task_Manage_Client(Task model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.TaskTypes = new SelectList(Enum.GetValues(typeof(TaskType)));
                ViewBag.Users = _db.Users.Select(u => new { u.Id, FullName = u.FullName }).ToList();
                ViewBag.Projects = _db.Projects.Select(p => new { p.Id, p.Name }).ToList();
                TempData["ErrorMessage"] = "Invalid Inputs.";
                return View(model);
            }

            string userEmail = User.Identity.Name;
            var currentUser = _db.Users.FirstOrDefault(u => u.Email == userEmail);

            if (model.Id == 0)
            {
                model.CreatedById = currentUser.Id;
                model.CreatedDate = DateTime.UtcNow;
                TempData["SuccessMessage"] = "Task created successfully.";
                _db.Tasks.Add(model);
            }
            else
            {
                var existingTask = _db.Tasks.Find(model.Id);
                if (existingTask == null)
                {
                    TempData["ErrorMessage"] = "Task not found.";
                    return RedirectToAction("Tasks_List_Client", "Task");
                }

                existingTask.Title = model.Title;
                existingTask.Description = model.Description;
                existingTask.Status = model.Status;
                existingTask.StartDate = model.StartDate;
                existingTask.DueDate = model.DueDate;
                existingTask.Priority = model.Priority;
                existingTask.TaskType = model.TaskType;
                existingTask.IsCompleted = model.IsCompleted;
                existingTask.UpdatedDate = DateTime.UtcNow;

                existingTask.AssignedToId = model.AssignedToId;
                existingTask.ProjectId = model.ProjectId;

                _db.Entry(existingTask).State = EntityState.Modified;
                TempData["SuccessMessage"] = "Task updated successfully.";

            }

            try
            {
                _db.SaveChanges();
            }
            catch 
            {
                TempData["ErrorMessage"] = "An error occurred while saving the task.";
                ViewBag.TaskTypes = new SelectList(Enum.GetValues(typeof(TaskType)));
                ViewBag.Users = _db.Users.Select(u => new { u.Id, FullName = u.FullName }).ToList();
                ViewBag.Projects = _db.Projects.Select(p => new { p.Id, p.Name }).ToList();
                return View(model);
            }

            return RedirectToAction("Tasks_List_Client", "Task");
        }

        [AuthorizeRole("Client")]
        public ActionResult Task_Delete_Client(int id)
        {
            try
            {
                var task = _db.Tasks.Find(id);
                if (task == null)
                {
                    TempData["ErrorMessage"] = "Task not found.";
                    return RedirectToAction("Tasks_List_Client", "Task");
                }

                return PartialView("_Task_Remove_Client", task);
            }
            catch
            {
                TempData["ErrorMessage"] = "An error occurred while trying to delete the task.";
                return RedirectToAction("Tasks_List_Client", "Task");
            }
        }

        [AuthorizeRole("Client")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Task_DeleteConfirmed_Client(int id)
        {
            try
            {
                var task = _db.Tasks.Find(id);
                if (task == null)
                {
                    TempData["ErrorMessage"] = "Task not found.";
                    return RedirectToAction("Tasks_List_Client", "Task");
                }


                _db.Tasks.Remove(task);
                _db.SaveChanges();

                TempData["SuccessMessage"] = "Task deleted successfully.";
                return RedirectToAction("Tasks_List_Client", "Task");
            }
            catch 
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the task.";
                return RedirectToAction("Tasks_List_Client", "Task");
            }
        }
    }
}

