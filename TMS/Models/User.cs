using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TMS.Models
{
    public class User
    {
        public int Id { get; set; } // Primary Key

        [Required, MaxLength(100)]
        public string FullName { get; set; } // User's Full Name

        [Required, EmailAddress]
        public string Email { get; set; } // User Email for login/identification

        [Required]
        [StringLength(64, MinimumLength = 8, ErrorMessage = "Password must be minimum of 8 characters.")]
        public string PasswordHash { get; set; }

        [Required]
        public bool IsActive { get; set; } = true; // True by default when a new user is created

        // Primary Role - Admin or Client
        [Required(ErrorMessage = "The Role field is required.")]
        [ForeignKey("Role")]
        public int RoleId { get; set; }
        public virtual Role Role { get; set; }

        // Navigation for tracking task ownership
        public virtual ICollection<Task> CreatedTasks { get; set; } // Tasks created by the user
        public virtual ICollection<Task> AssignedTasks { get; set; } // Tasks assigned to the user

        // Tracks the Admin who created this User (for Admin-created Users)
        [ForeignKey("CreatedByAdmin")]
        public int? CreatedByAdminId { get; set; }
        public virtual User CreatedByAdmin { get; set; }

        // ✅ NEW FIELD TO TRACK FIRST LOGIN
        public bool IsNewUser { get; set; } = true;
    }
}