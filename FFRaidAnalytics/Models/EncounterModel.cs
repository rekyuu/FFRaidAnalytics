using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FFRaidAnalytics.Models
{
    public class EncounterModel
    {
        [Key] public long Id { get; set; }
        public string EncounterName { get; set; }
    }
}
