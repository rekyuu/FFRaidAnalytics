using System;
using System.ComponentModel.DataAnnotations;

namespace FFRaidAnalytics.Models
{
    public class ReportModel
    {
        [Key] public string Code { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
