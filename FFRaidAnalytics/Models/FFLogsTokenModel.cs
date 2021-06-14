using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace FFRaidAnalytics.Models
{
    public class FFLogsTokenModel
    {
        [Key] public long Id { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("expires_in")]
        public long ExpiresIn { get; set; }

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        public DateTime TokenObtainedAt { get; set; }

        public DateTime TokenExpiresAt { get { return TokenObtainedAt.AddSeconds(ExpiresIn); } }

        public DateTime? RateLimitResetsAt { get; set; }

        public FFLogsTokenModel()
        {
            Id = 1;
            TokenObtainedAt = DateTime.UtcNow;
        }
    }
}
