using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TMS.Models
{
    public enum TaskType
    {
        // General Task Types
        Development,          // General development-related tasks
        Design,               // Design-related tasks
        Testing,              // Testing and QA tasks
        Research,             // Research-related tasks
        Documentation,        // Documentation tasks
        Deployment,           // Deployment tasks
        BugFix,               // Bug fixing tasks
        Maintenance,          // Maintenance-related tasks
        Training,             // Training tasks for staff or users
        Analysis,             // Data analysis tasks
        Marketing,            // Marketing-related tasks
        Configuration,        // Configuration tasks for systems or software
        Support,              // Customer support or system support tasks

        // VA (Virtual Assistant) Task Types
        EmailManagement,      // Managing emails (sorting, responding, etc.)
        CalendarManagement,   // Managing and scheduling appointments
        DataEntry,            // Data entry and management tasks
        SocialMedia,          // Social media management tasks
        CustomerSupport,      // Customer support-related tasks (phone, chat, etc.)
        Scheduling,           // Scheduling appointments or meetings
        ResearchAssistance,   // Assisting with online research or tasks
        ContentCreation,      // Creating content for websites, blogs, etc.
        Transcription,        // Transcribing audio or video content
        VirtualAssistance,    // General virtual assistant tasks
        TravelManagement,     // Managing travel plans and bookings
        EventPlanning,        // Planning and organizing events
        OnlineSupport,        // Online-based support tasks (chat, email support)
    }

    public class Task
    {
        public int Id { get; set; } // Primary Key

        [Required, MaxLength(100)]
        public string Title { get; set; } // Task Title

        [AllowHtml]
        [MaxLength(500)]
        public string Description { get; set; } // Task Description

        [Required]
        public DateTime StartDate { get; set; } // Start Date

        [Required]
        public DateTime DueDate { get; set; } // Task Deadline

        [Required]
        public string Status { get; set; } // Status (Pending, In Progress, Completed)

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now; // Task creation timestamp

        public DateTime? UpdatedDate { get; set; } // Last updated timestamp

        [Required]
        public string Priority { get; set; } // Priority (Low, Medium, High)

        [Required]
        public TaskType TaskType { get; set; } // Task Type (Single Dropdown Enum)

        public bool IsCompleted { get; set; } = false; // Simplifies status checks

        // Comments and Attachments
        public virtual ICollection<string> Comments { get; set; } // Task comments
        public virtual ICollection<string> Attachments { get; set; } // Task attachments

        // Task Creator (Admin or Client)
        [Required, ForeignKey("CreatedBy")]
        public int CreatedById { get; set; }
        public virtual User CreatedBy { get; set; }

        // Assigned To (Only Clients or VAs)
        [ForeignKey("AssignedTo")]
        public int? AssignedToId { get; set; }
        public virtual User AssignedTo { get; set; }

        // Project Relationship (Optional)
        [ForeignKey("Project")]
        public int? ProjectId { get; set; }
        public virtual Project Project { get; set; }

        [MaxLength(1000)]
        public string TaskNotes { get; set; } // Notes or messages to the Admin

        public string TransferredBy { get; set; } // The user who transferred the task
        public string TransferredTo { get; set; } // The user to whom the task is transferred
        public DateTime? TransferredDate { get; set; } // The date of the transfer
    }

}