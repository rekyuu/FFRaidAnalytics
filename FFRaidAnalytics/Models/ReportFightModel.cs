using System;
using System.ComponentModel.DataAnnotations;

namespace FFRaidAnalytics.Models
{
    public class ReportFightModel
    {
        public string ReportCode { get; set; }
        public long FightNo { get; set; }
        public long EncounterId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public long DurationMs { get; set; }
        public double FightPercentage { get; set; }
        public bool Kill { get; set; }
    }
}
