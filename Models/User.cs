using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace events_planner.Models
{
    [Table("user")]
    public class User
    {
        [Column("user_id")]
        [Key]
        public int Id { get; set; }

        [Column("username")]
        [StringLength(20, MinimumLength = 3)]
        [MaxLength(20, ErrorMessage = "Username must be under 20 characters")]
        [MinLength(3, ErrorMessage = "Username must be at least 3 characters")]
        public string Username { get; set; }

        [Column("first_name")]
        [StringLength(20, MinimumLength = 3)]
        [MaxLength(20, ErrorMessage = "First Name must be under 20 characters")]
        [MinLength(3, ErrorMessage = "First Name must be at least 3 characters")]
        [Required]
        public string FirstName { get; set; }

        [Column("last_name")]
        [StringLength(20, MinimumLength = 3)]
        [MaxLength(20, ErrorMessage = "Last Name must be under 20 characters")]
        [MinLength(3, ErrorMessage = "Last Name must be at least 3 characters")]
        [Required]
        public string LastName { get; set; }

        [Column("email")]
        [StringLength(30, MinimumLength = 3)]
        [ConcurrencyCheck]
        [Required]
        public string Email { get; set; }

        [Column("date_of_birth")]
        public DateTime DateOfBirth { get; set; }

        [Column("password")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        [Required]
        public string Password { get; set; }

        [Column("phone_number")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        [Required]
        public int PhoneNumber { get; set; }

        [Column("created_at")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdatedAt { get; set; }

        public ICollection<Booking> Bookings { get; set; }

        public ICollection<Subscribe> SubscribeTo { get; set; }

        [Column("promotion_id")]
        [ForeignKey("promotion_id")]
        public int PromotionId { get; set; }

        public Promotion Promotion { get; set; }

        [Column("role_id")]
        [ForeignKey("role_id")]
        public int RoleId { get; set; }

        public Role Role { get; set; }

        [ForeignKey("recovery_id")]
        public virtual Recovery Recovery { get; set; };
    }
}