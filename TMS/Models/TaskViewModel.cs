using System;
using System.Collections.Generic;

namespace TMS.Models
{
    public class TaskViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string TaskType { get; set; }
        public string CreatedBy { get; set; }
        public string AssignedTo { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public string Description { get; set; } // Task Description
        public string TaskNotes { get; set; } // Additional notes
        public int ProjectId { get; set; } // To associate task with a project
        public DateTime CreatedDate { get; set; } // Task creation date
        public DateTime? UpdatedDate { get; set; } // Task last update date
        public string CompletedBy { get; set; } // Full name of the person who completed the task
        public DateTime? CompletedDate { get; set; } // The date the task was completed
                                                     // New Transfer Details
        public string TransferredBy { get; set; }
        public string TransferredTo { get; set; }
        public DateTime? TransferredDate { get; set; }
    }
}
