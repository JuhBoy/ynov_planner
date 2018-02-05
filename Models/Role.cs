using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace events_planner.Models
{
    [Table("role")]
    public class Role
    {
        [Column("role_id")]
        [Key]
        public int Id { get; set; }

        [Column("name")]
        [StringLength(30, MinimumLength = 3)]
        [MaxLength(30, ErrorMessage = "Name's Role must be under 20 characters")]
        [MinLength(3, ErrorMessage = "Name's Role must be at least 3 characters")]
        [Required]
        public string Name { get; set; }
    }
}