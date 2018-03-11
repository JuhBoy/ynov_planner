using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace events_planner.Models
{
    [Table("category")]
    public class Category
    {
        [Column("category_id")]
        [Key]
        public int Id { get; set; }

        [Column("name")]
        [StringLength(20, MinimumLength = 2)]
        [MaxLength(20, ErrorMessage = "Username must be under 20 characters")]
        [MinLength(2, ErrorMessage = "Username must be at least 2 characters")]
        public string Name { get; set; }

        [Column("created_at"), JsonIgnore]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at"), JsonIgnore]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdatedAt { get; set; }

        /***********************
            RELATIONS
        ************************/

        /// <summary> relation with category - self loop ! (One to One) </summary>
        [Column("sub_category_id"), JsonIgnore]
        [ForeignKey("sub_category_id")]
        public int? SubCategoryId { get; set; }
        [JsonIgnore] public Category SubCategory { get; set; }

        /// <summary> relation with subcriber (One to Many) </summary>
        [JsonIgnore]
        public ICollection<Subscribe> Subscribers { get; set; }

        /// <summary> relation with Event (Many to Many) </summary>
        [JsonIgnore]
        public IList<EventCategory> EventCategory { get; set; }
    }
}