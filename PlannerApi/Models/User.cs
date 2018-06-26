using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace events_planner.Models
{
    [Table("user")]
    public class User {
        [Column("user_id")]
        [Key]
        public int Id { get; set; }

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
        public DateTime? DateOfBirth { get; set; }

        [Column("password")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        [Required]
        public string Password { get; set; }

        [Column("phone_number")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        [Required, Phone]
        public int PhoneNumber { get; set; }

        [Column("created_at")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdatedAt { get; set; }

        /***********************
            RELATIONS
        ************************/

        /// <summary> relation with Booking (One to Many) </summary>
        [JsonIgnore]
        public ICollection<Booking> Bookings { get; set; }

        /// <summary> relation with Subcribe (One to Many) </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<Subscribe> SubscribeTo { get; set; }

        /// <summary> relation with Promotion (Many to One) </summary>
        [Column("promotion_id")]
        [ForeignKey("promotion_id")]
        public int PromotionId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Promotion Promotion { get; set; }

        /// <summary> relation with Role (Many to One) </summary>
        [Column("role_id"), ForeignKey("role_id"), JsonIgnore]
        public int RoleId { get; set; }

        /// <summary> relation with Recovery (One to One) </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Role Role { get; set; }

        [ForeignKey("recovery_id")]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public virtual Recovery Recovery { get; set; }

        /// <summary> relation with Event (Many to Many) </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IList<EventUser> EventUser { get; set; }

        /// <summary> Cumulated jury points </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IList<JuryPoint> JuryPoint { get; set; }

        /// <summary> Temporary roles given to not admin user </summary>
        [JsonIgnore]
        public IList<TemporaryRole> TemporaryRoleId { get; set; }

        #region Public HELPERS

        [NotMapped]
        public int? TotalJuryPoints {
            get {
                if (JuryPoint == null) return null;
                int total = 0;
                foreach (int u in JuryPoint.Select(s => s.Points))
                    total += u;
                return total;
            }
        }

        [NotMapped]
        public string FullName {
            get {
                return FirstName + " " + LastName;
            }
        }

        #endregion
    }
}