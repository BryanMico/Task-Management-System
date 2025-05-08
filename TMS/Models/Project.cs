using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TMS.Models
{
    public class Project
    {
        public int Id { get; set; } // Primary Key

        [Required(ErrorMessage = "Project Name is required.")]
        [MaxLength(100, ErrorMessage = "Project Name cannot exceed 100 characters.")]
        public string Name { get; set; } // Project Title

        [MaxLength(300, ErrorMessage = "Description cannot exceed 300 characters.")]
        public string Description { get; set; } // Project Description

        [Required(ErrorMessage = "Project Code is required.")]
        [MaxLength(20, ErrorMessage = "Project Code cannot exceed 20 characters.")]
        public string ProjectCode { get; set; } // Unique Project Identifier (e.g., "PRJ-2025-001")

        [Required(ErrorMessage = "Project creation date is required.")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Project Creation Date

        public DateTime? DueDate { get; set; } // Deadline for the project

        public DateTime? UpdatedAt { get; set; } // Last Modified Date

        [RegularExpression("^(Active|Completed|On Hold)$", ErrorMessage = "Status must be one of the following: Active, Completed, On Hold.")]
        [Required(ErrorMessage = "Project Status is required.")]
        [MaxLength(20, ErrorMessage = "Project Status cannot exceed 20 characters.")]
        public string Status { get; set; } = "Active"; // Project Status (e.g., "Active", "Completed", "On Hold")

        public virtual ICollection<Task> Tasks { get; set; }

        [Required(ErrorMessage = "The Admin who created the project is required.")]
        [ForeignKey("CreatedBy")]
        public int CreatedById { get; set; }
        public virtual User CreatedBy { get; set; }
    }
}
