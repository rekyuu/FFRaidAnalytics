using FFRaidAnalytics.Db;
using FFRaidAnalytics.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FFRaidAnalytics.Controllers
{
    [Route("encounters")]
    public class EncounterController : Controller
    {
        private readonly FFRaidAnalyticsContext _context;

        public EncounterController(FFRaidAnalyticsContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            IEnumerable<EncounterModel> encounters = _context.Encounters
                .OrderBy(x => x.EncounterName);

            ViewData["Title"] = "Encounters";

            return View(encounters);
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> Details(long id)
        {
            EncounterModel encounter = await _context.Encounters.FindAsync(id);

            if (encounter == null) return NotFound();

            IEnumerable<ReportFightModel> fights = _context.ReportFights
                .Where(x => x.EncounterId == id);

            IEnumerable<object> reportFights = fights
                .Select(x => new { x.ReportCode, x.FightNo })
                .Distinct();

            long maxDuration = fights.Max(x => x.DurationMs);
            double avgDuration = fights.Average(x => x.DurationMs);
            long totalDuration = fights.Sum(x => x.DurationMs);

            IEnumerable<ReportFightPlayerModel> playerFights = _context.ReportFightPlayers
                .AsEnumerable()
                .Where(x => reportFights.Contains(new { x.ReportCode, x.FightNo }));

            IEnumerable<long> reportPlayers = playerFights
                .Select(x => x.PlayerId)
                .Distinct();

            IEnumerable<EncounterViewPlayerModel> players = _context.Players
                .AsEnumerable()
                .Where(player => reportPlayers.Contains(player.Id))
                .Select(player => new EncounterViewPlayerModel()
                {
                    Name = player.Name.ToLower(),
                    AverageDPS = playerFights
                        .Where(fight => fight.PlayerId == player.Id)
                        .Sum(fight => fight.DamageDone) / (totalDuration / 1000),
                    AverageHPS = playerFights
                        .Where(fight => fight.PlayerId == player.Id)
                        .Sum(fight => fight.HealingDone) / (totalDuration / 1000),
                    Deaths = playerFights
                        .Where(fight => fight.PlayerId == player.Id)
                        .Sum(fight => fight.Deaths)
                });

            ViewData["Title"] = encounter.EncounterName;

            ViewData["Pulls"] = fights.Count();
            ViewData["MaxDuration"] = TimeSpan.FromMilliseconds(maxDuration);
            ViewData["AvgDuration"] = TimeSpan.FromMilliseconds(avgDuration);
            ViewData["TotalDuration"] = TimeSpan.FromMilliseconds(totalDuration);
            ViewData["Fights"] = fights;
            ViewData["PlayerData"] = players.OrderByDescending(x => x.AverageDPS).ToList();

            return View();
        }
    }
}
