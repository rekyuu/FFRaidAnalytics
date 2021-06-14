using FFRaidAnalytics.Models;
using Microsoft.EntityFrameworkCore;

namespace FFRaidAnalytics.Db
{
    public class FFRaidAnalyticsContext : DbContext
    {
        public DbSet<ReportModel> Reports { get; set; }
        public DbSet<ReportFightModel> ReportFights { get; set; }
        public DbSet<ReportFightPlayerModel> ReportFightPlayers { get; set; }
        public DbSet<PlayerModel> Players { get; set; }
        public DbSet<EncounterModel> Encounters { get; set; }
        public DbSet<FFLogsTokenModel> Token { get; set; }

        public FFRaidAnalyticsContext() : base() { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ReportFightModel>().HasKey(x => new
            {
                x.ReportCode,
                x.FightNo
            });

            modelBuilder.Entity<ReportFightPlayerModel>().HasKey(x => new
            {
                x.ReportCode,
                x.FightNo,
                x.PlayerId
            });

            modelBuilder.Entity<PlayerModel>().HasIndex(x => new
            {
                x.Name,
                x.Server
            }).IsUnique();

            modelBuilder.Entity<ReportFightModel>().HasIndex(x => new
            {
                x.EncounterId
            });

            modelBuilder.Entity<ReportFightModel>().HasIndex(x => new
            {
                x.StartTime
            });
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(@"Data Source=/data/FFRaidAnalytics.db");
            //optionsBuilder.UseInMemoryDatabase("FFRaidAnalytics");
        }
    }
}
