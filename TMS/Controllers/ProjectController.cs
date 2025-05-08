using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using TMS.Models;
using PagedList;
using System.Net;
using TMS.Filters;

namespace TMS.Controllers
{
    public class ProjectController : Controller
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
        public ActionResult Projects_List(string searchQueryProject, string statusFilter, int? page)
        {
            try
            {
                SetLoggedInUserDetails();

                int pageSize = 6;
                int pageNumber = (page ?? 1);

                var projects = _db.Projects
                    .Include(p => p.CreatedBy)
                    .Select(p => new ProjectViewModel
                    {
                        Id = p.Id,
                        ProjectCode = p.ProjectCode,
                        Name = p.Name,
                        Description = p.Description,
                        CreatedBy = p.CreatedBy != null ? p.CreatedBy.FullName : "Unknown",
                        CreatedAt = p.CreatedAt,
                        DueDate = p.DueDate,
                        Status = p.Status
                    }).ToList();

                if (!projects.Any())
                {
                    TempData["ErrorMessage"] = "No projects found.";
                    return View(new List<ProjectViewModel>().ToPagedList(pageNumber, pageSize));
                }

                if (!string.IsNullOrEmpty(statusFilter))
                {
                    projects = projects.Where(p => p.Status == statusFilter).ToList();
                }

                if (!string.IsNullOrEmpty(searchQueryProject))
                {
                    projects = projects.Where(p => p.Name.Contains(searchQueryProject) || p.ProjectCode.Contains(searchQueryProject)).ToList();
                }

                ViewBag.SearchQuery = searchQueryProject;
                ViewBag.StatusFilter = statusFilter;

                return View(projects.OrderBy(p => p.CreatedAt).ToPagedList(pageNumber, pageSize));
            }
            catch
            {
                TempData["ErrorMessage"] = "An error occurred while retrieving the project list. Please try again.";
                return RedirectToAction("Dashboard_Admin", "Dashboard");
            }
        }

        private string GenerateProjectCode()
        {
            try
            {
                string year = DateTime.Now.Year.ToString();
                int count = _db.Projects.Count(p => p.CreatedAt.Year == DateTime.Now.Year) + 1;
                return $"PROJ-{year}-{count:D3}";
            }
            catch
            {
                TempData["ErrorMessage"] = "An error occurred while generating the project code.";
                return "PROJ-ERROR";
            }
        }

        [AuthorizeRole("Admin")]
        public ActionResult Project_Manage(int? id)
        {
            try
            {
                SetLoggedInUserDetails();

                var project = id.HasValue ? _db.Projects.Find(id) : new Project();
                if (project == null && id.HasValue)
                {
                    TempData["ErrorMessage"] = "Project not found.";
                    return RedirectToAction("Projects_List", "Project");
                }

                if (project.Id == 0)
                {
                    project.ProjectCode = GenerateProjectCode();
                }

                return View(project);
            }
            catch
            {
                TempData["ErrorMessage"] = "An error occurred while loading the project details.";
                return RedirectToAction("Projects_List", "Project");
            }
        }

        [AuthorizeRole("Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Project_Manage(Project model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Please check your inputs and try again.";
                    return View(model);
                }



                string userEmail = User.Identity.Name;
                var currentUser = _db.Users.FirstOrDefault(u => u.Email == userEmail);

                if (currentUser == null || currentUser.Role.Name != "Admin")
                {
                    TempData["ErrorMessage"] = "Only Admins can create or edit projects.";
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }

                if (model.Id == 0)
                {
                    // Creating a new project
                    model.CreatedById = currentUser.Id;
                    model.CreatedAt = DateTime.UtcNow;
                    _db.Projects.Add(model);
                    TempData["SuccessMessage"] = "Project created successfully.";
                }
                else
                {
                    // Editing an existing project
                    var existingProject = _db.Projects.Find(model.Id);
                    if (existingProject == null)
                    {
                        TempData["ErrorMessage"] = "Project not found.";
                        return RedirectToAction("Projects_List", "Project");
                    }

                    existingProject.Name = model.Name;
                    existingProject.ProjectCode = model.ProjectCode;
                    existingProject.Description = model.Description;
                    existingProject.Status = model.Status;
                    existingProject.DueDate = model.DueDate;

                    _db.Entry(existingProject).State = EntityState.Modified;
                    TempData["SuccessMessage"] = "Project updated successfully.";
                }

                _db.SaveChanges();
                return RedirectToAction("Projects_List", "Project");
            }
            catch
            {
                TempData["ErrorMessage"] = "An error occurred while saving the project. Please try again.";
                return View(model);
            }
        }


        [AuthorizeRole("Admin")]
        public ActionResult Project_Delete(int id)
        {
            try
            {
                var project = _db.Projects.Find(id);
                if (project == null)
                {
                    TempData["ErrorMessage"] = "Project not found.";
                    return RedirectToAction("Projects_List", "Project");
                }

                return PartialView("_Project_Remove", project);
            }
            catch
            {
                TempData["ErrorMessage"] = "An error occurred while trying to delete the project.";
                return RedirectToAction("Projects_List", "Project");
            }
        }

        [AuthorizeRole("Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Project_DeleteConfirmed(int id)
        {
            try
            {
                var project = _db.Projects.Find(id);
                if (project == null)
                {
                    TempData["ErrorMessage"] = "Project not found.";
                    return RedirectToAction("Projects_List", "Project");
                }


                var tasks = _db.Tasks.Where(t => t.ProjectId == project.Id).ToList();
                if (tasks.Any())
                {
                    TempData["ErrorMessage"] = "Cannot delete project. It has one or more tasks.";
                    return RedirectToAction("Projects_List", "Project");
                }

                _db.Projects.Remove(project);
                _db.SaveChanges();

                TempData["SuccessMessage"] = "Project deleted successfully.";
                return RedirectToAction("Projects_List", "Project");
            }
            catch
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the project.";
                return RedirectToAction("Projects_List", "Project");
            }
        }


        [AuthorizeRole("Admin")]
        public ActionResult Project_View(int? page, int? projectId, string searchQueryTask, string statusFilter)
        {
            try
            {
                SetLoggedInUserDetails();

                var users = _db.Users.ToList();
                ViewBag.Users = users;

                if (projectId == null)
                {
                    ViewBag.SelectedProject = "Select a Project";
                    return View(new PagedList<TaskViewModel>(new List<TaskViewModel>(), 1, 10)); // Empty list
                }

                var selectedProject = _db.Projects.Find(projectId);
                if (selectedProject == null)
                {
                    ViewBag.SelectedProject = "Project Not Found";
                    return View(new PagedList<TaskViewModel>(new List<TaskViewModel>(), 1, 10)); // Empty list
                }

                // Store project details in ViewBag to display in the view
                ViewBag.SelectedProject = selectedProject.Name;
                ViewBag.SelectedProjectId = projectId;
                ViewBag.ProjectDetails = new ProjectViewModel
                {
                    Id = selectedProject.Id,
                    ProjectCode = selectedProject.ProjectCode,
                    Name = selectedProject.Name,
                    Description = selectedProject.Description,
                    CreatedBy = selectedProject.CreatedBy.FullName,
                    CreatedAt = selectedProject.CreatedAt,
                    DueDate = selectedProject.DueDate,
                    Status = selectedProject.Status
                };

                // Filter tasks based on the selected project
                var tasksQuery = _db.Tasks.Where(t => t.ProjectId == projectId);

                // Apply optional search filtering
                if (!string.IsNullOrEmpty(searchQueryTask))
                {
                    tasksQuery = tasksQuery.Where(t => t.Title.Contains(searchQueryTask));
                }

                // Apply optional status filtering
                if (!string.IsNullOrEmpty(statusFilter))
                {
                    tasksQuery = tasksQuery.Where(t => t.Status == statusFilter);
                }

                // Convert to ViewModel for display
                var tasks = tasksQuery
                    .Select(t => new TaskViewModel
                    {
                        Title = t.Title,
                        TaskType = t.TaskType.ToString(),
                        CreatedBy = t.CreatedBy != null ? t.CreatedBy.FullName : "Unknown",
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
            catch
            {
                TempData["ErrorMessage"] = "An error occurred while loading the project details. Please try again.";
                return RedirectToAction("Projects_List", "Project");
            }
        }


    }
}