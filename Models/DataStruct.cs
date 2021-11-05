using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrismMessageBox.Models
{
   
    public class SingleTask
    {
        public int Order { get; set; } = 0;
        public string TaskName { get; set; } = "TaskName";
        public string TaskContent { get; set; } = "TaskContent";
    }
    public class Plans
    {
        public string PlanName { get; set; } = "planName";
        public List<SingleTask> FixedTask { get; set; } = new List<SingleTask>();
        public List<SingleTask> UnfixedTask { get; set; } = new List<SingleTask>();
    }
}
