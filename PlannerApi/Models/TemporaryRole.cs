﻿using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace events_planner.Models {
    
    [Table("temporary_role")]
    public class TemporaryRole {

        [Column("id")]
        public int Id { get; set; }

        [Column("event_id"), ForeignKey("event_id"), JsonIgnore]
        public int EventId { get; set; }

        [JsonIgnore]
        public Event Event { get; set; }

        [Column("user_id"), ForeignKey("user_id"), JsonIgnore]
        public int UserId { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public User User { get; set; }

        [ForeignKey("role_id"), Column("role_id"), JsonIgnore]
        public int RoleId { get; set; }

        [JsonIgnore]
        public Role Role { get; set; }
    }
}
