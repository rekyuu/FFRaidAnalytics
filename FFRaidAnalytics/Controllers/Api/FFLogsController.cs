using FFRaidAnalytics.Db;
using FFRaidAnalytics.Models;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net;
using System.Threading;

namespace FFRaidAnalytics.Controllers.Api
{
    [Route("api/fflogs")]
    [ApiController]
    public class FFLogsController : ControllerBase
    {
        private readonly FFRaidAnalyticsContext _context;
        private readonly GraphQLHttpClient _graphql = new("https://www.fflogs.com/api/v2/client", new NewtonsoftJsonSerializer());
        private readonly HttpClient _http = new();

        public FFLogsController(FFRaidAnalyticsContext context)
        {
            _context = context;
        }

        [HttpGet("login")]
        public async Task<ActionResult<FFLogsTokenModel>> Login()
        {
            return await GetToken();
        }

        [HttpGet("reports")]
        public async Task<ActionResult<IEnumerable<ReportModel>>> GetReports(string code = "")
        {
            if (string.IsNullOrEmpty(code))
            {
                return await _context.Reports.OrderBy(x => x.StartTime).ToListAsync();
            }
            else
            {
                ReportModel report = await _context.Reports.FindAsync(code);

                if (report == null) return NotFound();

                return new List<ReportModel>() { report };
            }
        }

        [HttpPost("reports")]
        public async Task<ActionResult<object>> ProcessNewReports(DateTime? startDate = null, DateTime? endDate = null)
        {
            long reportsProcessed = 0;
            long fightsProcessed = 0;
            long playerFightDataProcessed = 0;

            if (endDate == null) endDate = DateTime.UtcNow;
            if (startDate == null) startDate = endDate.Value.AddDays(-1);

            long userId = Convert.ToInt64(Environment.GetEnvironmentVariable("FFLOGS_USER_ID"));

            // Initial query -- amount of reports and total pages
            Reports reportsInfo = await GetReportsInfo(userId, startDate.Value, endDate.Value);

            for (long page = reportsInfo.LastPage; page >= 1; page--)
            {
                // Paginated query -- list of reports
                Report[] reports = await GetReports(userId, startDate.Value, endDate.Value, page);

                foreach (Report report in reports.Reverse())
                {
                    // Insert report.
                    ReportModel reportModel = new()
                    {
                        Code = report.Code,
                        StartTime = DateTimeOffset.FromUnixTimeMilliseconds(report.StartTime).DateTime,
                        EndTime = DateTimeOffset.FromUnixTimeMilliseconds(report.EndTime).DateTime
                    };

                    await AddOrUpdate(reportModel, report.Code);

                    // List of players in the report.
                    Actor[] reportActors = await GetReportActors(report.Code);

                    Dictionary<long, long> actorToPlayerMap = new();
                    foreach (Actor actor in reportActors)
                    {
                        if (string.IsNullOrEmpty(actor.Name)) continue;
                        if (string.IsNullOrEmpty(actor.Server)) continue;

                        PlayerModel player = await GetPlayer(actor.Name, actor.Server);

                        actorToPlayerMap[actor.Id] = player.Id;
                    }

                    for (long fightNo = 1; fightNo <= report.Fights.Length; fightNo++)
                    {
                        Fight fight = report.Fights[fightNo - 1];

                        if (fight.EncounterId == 0) continue;

                        _ = GetEncounter(fight.EncounterId);

                        // Insert fight data.
                        ReportFightModel fightModel = new()
                        {
                            ReportCode = report.Code,
                            FightNo = fightNo,
                            EncounterId = fight.EncounterId,
                            StartTime = DateTimeOffset.FromUnixTimeMilliseconds(report.StartTime + fight.StartTime).DateTime,
                            EndTime = DateTimeOffset.FromUnixTimeMilliseconds(report.StartTime + fight.EndTime).DateTime,
                            DurationMs = fight.EndTime - fight.StartTime,
                            FightPercentage = fight.FightPercentage.Value,
                            Kill = fight.Kill.Value
                        };

                        await AddOrUpdate(fightModel, report.Code, fightNo);

                        // Insert player performace data.
                        TableData tableData = await GetReportTable(report.Code, fight.StartTime, fight.EndTime, fightNo);
                        foreach (Composition player in tableData.Composition)
                        {
                            long playerId = actorToPlayerMap[player.Id];

                            ReportFightPlayerModel playerFightModel = new()
                            {
                                ReportCode = report.Code,
                                FightNo = fightNo,
                                PlayerId = playerId,
                                Class = player.Type,
                                DamageDone = tableData.DamageDone
                                    .Where(x => x.Id == player.Id)
                                    .Sum(x => x.Total),
                                HealingDone = tableData.HealingDone
                                    .Where(x => x.Id == player.Id)
                                    .Sum(x => x.Total),
                                Deaths = tableData.DeathEvents
                                    .Where(x => x.Id == player.Id)
                                    .Count()
                            };

                            await AddOrUpdate(playerFightModel, report.Code, fightNo, playerId);

                            playerFightDataProcessed++;
                        }

                        fightsProcessed++;
                    }

                    reportsProcessed++;
                }
            }

            return new { reportsProcessed, fightsProcessed, playerFightDataProcessed };
        }

        [HttpGet("reports/fights")]
        public async Task<ActionResult<IEnumerable<ReportFightModel>>> GetReportFights(string code, long? fight = null)
        {
            if (fight == null)
            {
                return await _context.ReportFights
                    .Where(x => x.ReportCode == code)
                    .OrderBy(x => x.StartTime)
                    .ToListAsync();
            }
            else
            {
                ReportFightModel reportFight = await _context.ReportFights.FindAsync(code, fight);

                if (reportFight == null) return NotFound();

                return new List<ReportFightModel>() { reportFight };
            }
        }

        [HttpGet("reports/players")]
        public async Task<ActionResult<IEnumerable<ReportFightPlayerModel>>> GetReportFightPlayers(string code, long fight)
        {
            return await _context.ReportFightPlayers
                .Where(x => x.ReportCode == code && x.FightNo == fight)
                .ToListAsync();
        }

        [HttpGet("players")]
        public async Task<ActionResult<IEnumerable<PlayerModel>>> GetPlayers(long? id = null, string name = "", string server = "")
        {
            if (id != null)
            {
                PlayerModel player = await GetPlayer(id.Value);

                if (player == null) return NotFound();

                return new List<PlayerModel>() { player };
            }
            else if (!string.IsNullOrEmpty(name) || !string.IsNullOrEmpty(server))
            {
                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(server))
                {
                    return BadRequest("Both name and server are required.");
                }

                PlayerModel player = await GetPlayer(name, server);

                if (player == null) return NotFound();

                return new List<PlayerModel>() { player };
            }
            else
            {
                return await _context.Players.ToListAsync();
            }
        }

        [HttpGet("encounters")]
        public async Task<ActionResult<IEnumerable<EncounterModel>>> GetEncounters(long? id = null)
        {
            if (id == null)
            {
                return await _context.Encounters.ToListAsync();
            }
            else
            {
                EncounterModel encounter = await _context.Encounters.FindAsync(id);

                if (encounter == null) return NotFound();

                return new List<EncounterModel>() { encounter };
            }
        }

        private async Task<Reports> GetReportsInfo(long userID, DateTime startDate, DateTime endDate, long limit = 50)
        {
            long startTime = ((DateTimeOffset)startDate).ToUnixTimeMilliseconds();
            long endTime = ((DateTimeOffset)endDate).ToUnixTimeMilliseconds();

            FFLogsData data = await MakeFFLogsQuery<FFLogsData>(new GraphQLRequest
            {
                Query = @"
                query getReportsInfo($userID: Int, $startTime: Float, $endTime: Float, $limit: Int) {
                    reportData {
                        reports(userID: $userID, startTime: $startTime, endTime: $endTime, limit: $limit) {
                            total
                            last_page
                        }
                    }
                }",
                OperationName = "getReportsInfo",
                Variables = new
                {
                    userID,
                    startTime,
                    endTime,
                    limit
                }
            });

            return data.ReportData.Reports;
        }

        private async Task<Report[]> GetReports(long userID, DateTime startDate, DateTime endDate, long page, long limit = 50)
        {
            long startTime = ((DateTimeOffset)startDate).ToUnixTimeMilliseconds();
            long endTime = ((DateTimeOffset)endDate).ToUnixTimeMilliseconds();

            FFLogsData data = await MakeFFLogsQuery<FFLogsData>(new GraphQLRequest
            {
                Query = @"
                query getReports($userID: Int, $startTime: Float, $endTime: Float, $page: Int, $limit: Int) {
                    reportData {
                        reports(userID: $userID, startTime: $startTime, endTime: $endTime, page: $page, limit: $limit) {
                            data {
                                code
                                startTime
                                endTime
                                fights {
                                    encounterID
                                    startTime
                                    endTime
                                    fightPercentage
                                    kill
                                }
                            }
                        }
                    }
                }",
                OperationName = "getReports",
                Variables = new
                {
                    userID,
                    startTime,
                    endTime,
                    page,
                    limit
                }
            });

            return data.ReportData.Reports.ReportsData;
        }

        private async Task<Actor[]> GetReportActors(string reportCode)
        {
            FFLogsData data = await MakeFFLogsQuery<FFLogsData>(new GraphQLRequest
            {
                Query = @"
                query getReportMasterDataActors($reportCode: String) {
                    reportData {
                        report(code: $reportCode) {
                            masterData {
                                actors(type: ""Player"") {
                                    id
                                    name
                                    server
                                }
                            }
                        }
                    }
                }",
                OperationName = "getReportMasterDataActors",
                Variables = new
                {
                    reportCode
                }
            });

            if (data.ReportData.Report.MasterData == null) return Array.Empty<Actor>();

            return data.ReportData.Report.MasterData.Actors;
        }

        private async Task<PlayerModel> GetPlayer(long id)
        {
            return await _context.Players.FindAsync(id);
        }

        private async Task<PlayerModel> GetPlayer(string name, string server)
        {
            if (string.IsNullOrEmpty(name)) throw new Exception("Name must be specified.");
            if (string.IsNullOrEmpty(server)) throw new Exception("Server must be specified");

            name = name.ToUpper();
            server = server.ToUpper();

            if (server == "EXCALIBUR")
            {
                if (name == "RE-KYUU SENKAN")
                {
                    server = "LAMIA";
                }

                if (name == "RECCOA LONDE")
                {
                    name = "CAIM DRAKENGARD";
                    server = "LAMIA";
                }
            }

            PlayerModel player = _context.Players.SingleOrDefault(x =>
                x.Name == name &&
                x.Server == server);

            if (player != null) return player;

            // Insert player data.
            player = new()
            {
                Name = name,
                Server = server
            };

            _context.Players.Update(player);
            await _context.SaveChangesAsync();

            return player;
        }

        private async Task<EncounterModel> GetEncounter(long encounterId)
        {
            EncounterModel encounter = await _context.Encounters.FindAsync(encounterId);

            if (encounter != null) return encounter;

            // Encounter query.
            FFLogsData data = await MakeFFLogsQuery<FFLogsData>(new GraphQLRequest
            {
                Query = @"
                query getEncounter($encounterId: Int) {
                    worldData {
                        encounter(id: $encounterId) {
                            name
                        }
                    }
                }",
                OperationName = "getEncounter",
                Variables = new
                {
                    encounterId
                }
            });

            // Insert encounter data.
            encounter = new()
            {
                Id = encounterId,
                EncounterName = data.WorldData.Encounter != null ? data.WorldData.Encounter.Name : "Unknown Encounter"
            };

            _context.Encounters.Add(encounter);
            await _context.SaveChangesAsync();

            return encounter;
        }

        private async Task<TableData> GetReportTable(string code, long startTime, long endTime, long fightNo)
        {
            // List of per-player performace data per fight
            FFLogsData data = await MakeFFLogsQuery<FFLogsData>(new GraphQLRequest
            {
                Query = @"
                query getReportTable($code: String, $startTime: Float, $endTime: Float, $fightId: Int) {
                    reportData {
                        report(code: $code) {
                            table(startTime: $startTime, endTime: $endTime, fightIDs: [$fightId])
                        }
                    }
                }",
                OperationName = "getReportTable",
                Variables = new
                {
                    code,
                    startTime,
                    endTime,
                    fightId = fightNo
                }
            });

            return data.ReportData.Report.Table.TableData;
        }

        private async Task<long> GetRateLimitReset(FFLogsTokenModel token)
        {
            // List of per-player performace data per fight
            FFLogsRateLimitData data = await MakeFFLogsQuery<FFLogsRateLimitData>(new GraphQLRequest
            {
                Query = @"
                query getRateLimits {
                    rateLimitData {
                        pointsResetIn
                    }
                }",
                OperationName = "getRateLimits"
            }, token);

            return data.RateLimitData.PointsResetIn;
        }

        private async Task<FFLogsTokenModel> UpdateTokenRateLimit(FFLogsTokenModel token)
        {
            long secondsToReset = await GetRateLimitReset(token);
            DateTime pointsResetAt = DateTime.UtcNow.AddSeconds(secondsToReset);

            token.RateLimitResetsAt = pointsResetAt;

            await AddOrUpdate(token, 1L);

            return token;
        }

        private async Task<FFLogsTokenModel> GetToken()
        {
            FFLogsTokenModel token = _context.Token.Find(1L);

            if (token != null)
            {
                if (token.RateLimitResetsAt != null)
                {
                    if (DateTime.UtcNow < token.TokenExpiresAt && DateTime.UtcNow < token.RateLimitResetsAt.Value)
                    {
                        return token;
                    }
                    else if (DateTime.UtcNow < token.TokenExpiresAt && DateTime.UtcNow > token.RateLimitResetsAt.Value)
                    {
                        return await UpdateTokenRateLimit(token);
                    }
                }
                else
                {
                    if (DateTime.UtcNow < token.TokenExpiresAt) return await UpdateTokenRateLimit(token);
                }
            }

            FormUrlEncodedContent content = new(new Dictionary<string, string>()
            {
                { "grant_type", "client_credentials" },
                { "client_id", Environment.GetEnvironmentVariable("FFLOGS_CLIENT_ID") },
                { "client_secret", Environment.GetEnvironmentVariable("FFLOGS_CLIENT_SECRET") },
            });

            _http.DefaultRequestHeaders.Add("cache-control", "no-cache");

            HttpResponseMessage response = await _http.PostAsync("https://www.fflogs.com/oauth/token", content);
            string responseContent = await response.Content.ReadAsStringAsync();

            token = JsonConvert.DeserializeObject<FFLogsTokenModel>(responseContent);

            await AddOrUpdate(token, 1L);

            return token;
        }

        private async Task<T> MakeFFLogsQuery<T>(GraphQLRequest request, FFLogsTokenModel token = null)
        {
            if (token == null) token = await GetToken();

            _graphql.HttpClient.DefaultRequestHeaders.Clear();
            _graphql.HttpClient.DefaultRequestHeaders.Add("authorization", $"Bearer {token.AccessToken}");

            while (true)
            {
                GraphQLResponse<T> response = await _graphql.SendQueryAsync<T>(request);

                if (response.AsGraphQLHttpResponse().StatusCode == HttpStatusCode.OK) return response.Data;

                TimeSpan timeToReset = token.RateLimitResetsAt.Value - DateTime.UtcNow;

                Thread.Sleep(Convert.ToInt32(timeToReset.TotalSeconds) * 1000);
            }
        }

        private async Task AddOrUpdate<T>(T model, params object[] key) where T : class
        {
            T dbEntry = await _context.Set<T>().FindAsync(key);

            if (dbEntry == null)
            {
                _context.Set<T>().Add(model);
            }
            else
            {
                _context.Entry(dbEntry).State = EntityState.Detached;
                _context.Set<T>().Update(model);
            }

            await _context.SaveChangesAsync();
        }
    }
}
