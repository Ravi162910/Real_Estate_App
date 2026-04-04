using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace Real_Estate_App.Models
{
    public class Agent
    {
        [Key]
        [BindNever]
        public int AgentID { get; set; }

        public string Agent_FullName { get; set; }
        public string Agent_AgencyName { get; set; }
    }
}
