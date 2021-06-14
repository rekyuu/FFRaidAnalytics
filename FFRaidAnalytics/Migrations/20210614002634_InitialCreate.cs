using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FFRaidAnalytics.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Encounters",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EncounterName = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Encounters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Server = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReportFightPlayers",
                columns: table => new
                {
                    ReportCode = table.Column<string>(type: "TEXT", nullable: false),
                    FightNo = table.Column<long>(type: "INTEGER", nullable: false),
                    PlayerId = table.Column<long>(type: "INTEGER", nullable: false),
                    Class = table.Column<string>(type: "TEXT", nullable: true),
                    DamageDone = table.Column<long>(type: "INTEGER", nullable: false),
                    HealingDone = table.Column<long>(type: "INTEGER", nullable: false),
                    Deaths = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportFightPlayers", x => new { x.ReportCode, x.FightNo, x.PlayerId });
                });

            migrationBuilder.CreateTable(
                name: "ReportFights",
                columns: table => new
                {
                    ReportCode = table.Column<string>(type: "TEXT", nullable: false),
                    FightNo = table.Column<long>(type: "INTEGER", nullable: false),
                    EncounterId = table.Column<long>(type: "INTEGER", nullable: false),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DurationMs = table.Column<long>(type: "INTEGER", nullable: false),
                    FightPercentage = table.Column<double>(type: "REAL", nullable: false),
                    Kill = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportFights", x => new { x.ReportCode, x.FightNo });
                });

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndTime = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "Token",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TokenType = table.Column<string>(type: "TEXT", nullable: true),
                    ExpiresIn = table.Column<long>(type: "INTEGER", nullable: false),
                    AccessToken = table.Column<string>(type: "TEXT", nullable: true),
                    TokenObtainedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RateLimitResetsAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Token", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Players_Name_Server",
                table: "Players",
                columns: new[] { "Name", "Server" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Encounters");

            migrationBuilder.DropTable(
                name: "Players");

            migrationBuilder.DropTable(
                name: "ReportFightPlayers");

            migrationBuilder.DropTable(
                name: "ReportFights");

            migrationBuilder.DropTable(
                name: "Reports");

            migrationBuilder.DropTable(
                name: "Token");
        }
    }
}
