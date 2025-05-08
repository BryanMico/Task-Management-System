using System;

namespace TMS.Models
{
    public class ProjectViewModel
    {
        public int Id { get; set; } // Project ID

        public string ProjectCode { get; set; } // Unique Code (e.g., PRJ-2025-001)

        public string Name { get; set; } // Project Name

        public string Description { get; set; } // Project Description

        public string CreatedBy { get; set; } // Admin who created the project (Full Name)

        public DateTime CreatedAt { get; set; } // Project Creation Date

        public DateTime? DueDate { get; set; } // Project Deadline (Nullable)

        public string Status { get; set; } // Project Status (Active, Completed, On Hold)

    }
}
