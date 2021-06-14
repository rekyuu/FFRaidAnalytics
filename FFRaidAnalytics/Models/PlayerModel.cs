using System;
using System.ComponentModel.DataAnnotations;

namespace FFRaidAnalytics.Models
{
    public class PlayerModel
    {
        [Key] public long Id { get; set; }
        public string Name { get; set; }
        public string Server { get; set; }
    }
}
