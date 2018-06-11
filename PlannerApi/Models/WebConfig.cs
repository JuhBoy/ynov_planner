using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace events_planner.Models {

    [Table("web_config")]
    public class WebConfig {

        [Key, Column("id"), JsonIgnore]
        public int Id { get; set; }

        [Required, Column("name")]
        public string Name { get; set; }

        [Column("subtitle")]
        public string Subtitle { get; set; }

        [Column("logo_url")]
        public string LogoUrl { get; set; }

        [Column("favicon_url")]
        public string FaviconUrl { get; set; }

        [Column("facebook_url")]
        public string FacebookUrl { get; set; }

        [Column("instagram_url")]
        public string InstagramUrl { get; set; }

        [Column("twitter_url")]
        public string TwitterUrl { get; set; }

        [Required, Column("legal_notice")]
        public string LegalNotice { get; set; } = "";

        [Required, Column("terms_of_usage")]
        public string TermsOfUsage { get; set; } = "";

        [Column("session_count"), JsonIgnore]
        public int SessionCount { get; set; } = 0;
    }

}
