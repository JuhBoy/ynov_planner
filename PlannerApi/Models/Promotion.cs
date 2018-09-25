using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace events_planner.Models
{
    [Table("promotion")]
    public class Promotion
    {
        [Column("promotion_id"), Key]
        public int Id { get; set; }

        [Column("name"), Required]
        public string Name { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("year"), Required]
        public string Year { get; set; } = DateTime.Now.Year.ToString();

        [Column("created_at")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdatedAt { get; set; }

        /***********************
            RELATIONS
        ************************/

        /// <summary> relation with User (One to Many) </summary>
        [JsonIgnore]
        public ICollection<User> Users { get; set; }

        /// <summary> relation with Event (Many to Many) </summary>
        [JsonIgnore]
        public IList<EventPromotion> EventPromotion { get; set; }
    }
}