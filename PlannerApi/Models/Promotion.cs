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
        [Column("promotion_id")]
        [Key]
        public int Id { get; set; }

        [Column("name")]
        [StringLength(40, MinimumLength = 3)]
        [MaxLength(40, ErrorMessage = "Name's Promotion must be under 40 characters")]
        [MinLength(3, ErrorMessage = "Name's Promotion must be at least 3 characters")]
        [Required]
        public string Name { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("year")]
        [Required]
        public string Year { get; set; }

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