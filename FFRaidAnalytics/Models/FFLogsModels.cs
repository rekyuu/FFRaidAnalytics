using Newtonsoft.Json;

namespace FFRaidAnalytics.Models
{
    public partial class FFLogsRateLimitData
    {
        [JsonProperty("rateLimitData")]
        public RateLimitData RateLimitData { get; set; }
    }

    public partial class RateLimitData
    {
        [JsonProperty("limitPerHour")]
        public long LimitPerHour { get; set; }

        [JsonProperty("pointsSpentThisHour")]
        public double PointsSpentThisHour { get; set; }

        [JsonProperty("pointsResetIn")]
        public long PointsResetIn { get; set; }
    }

    public partial class FFLogsData
    {
        [JsonProperty("reportData")]
        public ReportData ReportData { get; set; }

        [JsonProperty("worldData")]
        public WorldData WorldData { get; set; }
    }

    public partial class ReportData
    {
        [JsonProperty("reports")]
        public Reports Reports { get; set; }

        [JsonProperty("report")]
        public Report Report { get; set; }
    }

    public partial class Reports
    {
        [JsonProperty("total")]
        public long Total { get; set; }

        [JsonProperty("last_page")]
        public long LastPage { get; set; }

        [JsonProperty("data")]
        public Report[] ReportsData { get; set; }
    }

    public partial class Report
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("startTime")]
        public long StartTime { get; set; }

        [JsonProperty("endTime")]
        public long EndTime { get; set; }

        [JsonProperty("fights")]
        public Fight[] Fights { get; set; }

        [JsonProperty("table")]
        public Table Table { get; set; }

        [JsonProperty("masterData")]
        public MasterData MasterData { get; set; }
    }

    public partial class Fight
    {
        [JsonProperty("encounterID")]
        public long EncounterId { get; set; }

        [JsonProperty("startTime")]
        public long StartTime { get; set; }

        [JsonProperty("endTime")]
        public long EndTime { get; set; }

        [JsonProperty("fightPercentage")]
        public double? FightPercentage { get; set; }

        [JsonProperty("kill")]
        public bool? Kill { get; set; }
    }

    public partial class Table
    {
        [JsonProperty("data")]
        public TableData TableData { get; set; }
    }

    public partial class TableData
    {
        [JsonProperty("totalTime")]
        public long TotalTime { get; set; }

        [JsonProperty("itemLevel")]
        public long ItemLevel { get; set; }

        [JsonProperty("logVersion")]
        public long LogVersion { get; set; }

        [JsonProperty("gameVersion")]
        public long GameVersion { get; set; }

        [JsonProperty("composition")]
        public Composition[] Composition { get; set; }

        [JsonProperty("damageDone")]
        public TotalDone[] DamageDone { get; set; }

        [JsonProperty("healingDone")]
        public TotalDone[] HealingDone { get; set; }

        [JsonProperty("damageTaken")]
        public DamageTaken[] DamageTaken { get; set; }

        [JsonProperty("deathEvents")]
        public DeathEvent[] DeathEvents { get; set; }
    }

    public partial class Composition
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("guid")]
        public long Guid { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("specs")]
        public ActorSpec[] Specs { get; set; }
    }

    public partial class ActorSpec
    {
        [JsonProperty("spec")]
        public string Spec { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }
    }

    public partial class TotalDone
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("guid")]
        public long Guid { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("icon")]
        public string Icon { get; set; }

        [JsonProperty("total")]
        public long Total { get; set; }
    }

    public partial class DamageTaken
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("guid")]
        public long Guid { get; set; }

        [JsonProperty("type")]
        public long Type { get; set; }

        [JsonProperty("abilityIcon")]
        public string AbilityIcon { get; set; }

        [JsonProperty("total", NullValueHandling = NullValueHandling.Ignore)]
        public long? Total { get; set; }
    }

    public partial class DeathEvent
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("guid")]
        public long Guid { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("icon")]
        public string Icon { get; set; }

        [JsonProperty("deathTime")]
        public long DeathTime { get; set; }

        [JsonProperty("ability", NullValueHandling = NullValueHandling.Ignore)]
        public DamageTaken Ability { get; set; }
    }

    public partial class MasterData
    {
        [JsonProperty("actors")]
        public Actor[] Actors { get; set; }
    }

    public partial class Actor
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("server")]
        public string Server { get; set; }
    }

    public partial class WorldData
    {
        [JsonProperty("encounter")]
        public Encounter Encounter { get; set; }
    }

    public partial class Encounter
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}