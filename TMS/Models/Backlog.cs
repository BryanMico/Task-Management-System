using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TMS.Models
{
    public class Backlog
    {
        public int Id { get; set; } // Primary Key

        [Required]
        public string ActionType { get; set; } // Types: Created, Updated, Deleted, Transferred

        [Required]
        public string Details { get; set; } // Action details (e.g., Task title, user name)

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.Now; // Auto-timestamp

        // Reference to the user performing the action
        [Required, ForeignKey("User")]
        public int UserId { get; set; }
        public virtual User User { get; set; }
    }
}