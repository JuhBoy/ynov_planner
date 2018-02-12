using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace events_planner.Models
{
    [Table("subscribe")]
    public class Subscribe
    {
        [Column("subcribe_id")]
        [Key]
        public int Id { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdatedAt { get; set; }

        /***********************
            RELATIONS
        ************************/

        /// <summary> relation with User (Many to One) </summary>
        [Column("user_id")]
        [ForeignKey("user_id")]
        public int UserId { get; set; }

        public User User { get; set; }


        /// <summary> relation with Category (Many to One) </summary>
        [Column("category_id")]
        [ForeignKey("category_id")]
        public int CategoryId { get; set; }

        public Category Category { get; set; }
    }
}