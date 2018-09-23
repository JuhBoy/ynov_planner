using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace events_planner.Models {
    
    [Table("category")]
    public class Category {
        
        [Column("category_id")]
        [Key]
        public int Id { get; set; }

        [Column("name")]
        [MinLength(2, ErrorMessage = "Name must be at least 2 characters")]
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

        [ForeignKey("parent_category_id"), Column("parent_category_id"), 
         JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? ParentCategory { get; set; }

        [JsonIgnore]
        public Category Parent { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore,
                      DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IList<Category> SubsCategories { get; set; }

        /// <summary> relation with subcriber (One to Many) </summary>
        [JsonIgnore]
        public ICollection<Subscribe> Subscribers { get; set; }

        /// <summary> relation with Event (Many to Many) </summary>
        [JsonIgnore]
        public IList<EventCategory> EventCategory { get; set; }
    }
}