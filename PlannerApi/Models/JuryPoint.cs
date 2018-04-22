using System;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace events_planner.Models {

    [Table("jury_point")]
    public class JuryPoint {

        [Column("id"), Key]
        public int Id { get; set; }

        [Column("points")]
        public int Points { get; set; } = 0;

        [ForeignKey("user_id"), JsonIgnore]
        public int UserId { get; set; }

        public User User { get; set; }
    }
}
