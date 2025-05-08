using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TMS.Models
{
    public class DashboardViewModel
    {
        public string UserFullName { get; set; }
        public string UserEmail { get; set; }
        public int TotalTasks { get; set; }
        public int PendingTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int OnHoldTasks { get; set; }
        public bool IsNewUser { get; set; }
        public List<TaskViewModel> LatestTasks { get; set; }
    }
}