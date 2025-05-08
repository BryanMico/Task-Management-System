using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace TMS.Models
{
    public partial class TMS_DB : DbContext
    {
        public TMS_DB()
            : base("name=TMS")
        {
        }

        // DbSet properties to define database tables
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<Task> Tasks { get; set; }
        public virtual DbSet<Project> Projects { get; set; }
        public virtual DbSet<Backlog> Backlogs { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // ===============================
            // TASK - USER RELATIONSHIPS
            // ===============================
            modelBuilder.Entity<Task>()
                .HasRequired(t => t.CreatedBy)
                .WithMany(u => u.CreatedTasks)
                .HasForeignKey(t => t.CreatedById)
                .WillCascadeOnDelete(false); // Prevents accidental data loss

            modelBuilder.Entity<Task>()
                .HasOptional(t => t.AssignedTo)
                .WithMany(u => u.AssignedTasks)
                .HasForeignKey(t => t.AssignedToId)
                .WillCascadeOnDelete(false); // Prevents accidental data loss

            // ===============================
            // USER - CREATED BY ADMIN RELATIONSHIP
            // ===============================
            modelBuilder.Entity<User>()
                .HasOptional(u => u.CreatedByAdmin)
                .WithMany() // No navigation property needed in Admin
                .HasForeignKey(u => u.CreatedByAdminId)
                .WillCascadeOnDelete(false);


            // ===============================
            // TASK - PROJECT RELATIONSHIP
            // ===============================
            modelBuilder.Entity<Task>()
                .HasOptional(t => t.Project)
                .WithMany(p => p.Tasks)
                .HasForeignKey(t => t.ProjectId)
                .WillCascadeOnDelete(false);

            // ===============================
            // PROJECT - USER RELATIONSHIP
            // ===============================
            modelBuilder.Entity<Project>()
                .HasRequired(p => p.CreatedBy)
                .WithMany()
                .HasForeignKey(p => p.CreatedById)
                .WillCascadeOnDelete(false);

            // ===============================
            // BACKLOG - USER RELATIONSHIP
            // ===============================
            modelBuilder.Entity<Backlog>()
                .HasRequired(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .WillCascadeOnDelete(false);

            base.OnModelCreating(modelBuilder);
        }
    }
}
