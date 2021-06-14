using System;
using System.ComponentModel.DataAnnotations;

namespace FFRaidAnalytics.Models
{
    public class ReportFightPlayerModel
    {
        public string ReportCode { get; set; }
        public long FightNo { get; set; }
        public long PlayerId { get; set; }
        public string Class { get; set; }
        public long DamageDone { get; set; }
        public long HealingDone { get; set; }
        public long Deaths { get; set; }
    }
}
