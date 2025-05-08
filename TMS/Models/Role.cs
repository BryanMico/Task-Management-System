using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TMS.Models
{
    public class Role
    {
        public int Id { get; set; } // Primary Key

        [Required, MaxLength(50)]
        public string Name { get; set; } // Role name (Admin/Client)

        // Role can have multiple users
        public virtual ICollection<User> Users { get; set; }
    }
}