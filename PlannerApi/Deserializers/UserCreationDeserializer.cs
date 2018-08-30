using System;
using System.ComponentModel.DataAnnotations;

namespace events_planner.Deserializers
{
    public class UserCreationDeserializer
    {
        [StringLength(20, MinimumLength = 3), Required]
        [MaxLength(20, ErrorMessage = "First Name must be under 20 characters")]
        [MinLength(3, ErrorMessage = "First Name must be at least 3 characters")]
        public string FirstName { get; set; }

        [StringLength(20, MinimumLength = 3), Required]
        [MaxLength(20, ErrorMessage = "Last Name must be under 20 characters")]
        [MinLength(3, ErrorMessage = "Last Name must be at least 3 characters")]
        public string LastName { get; set; }

        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        [Required, Phone]
        public string PhoneNumber { get; set; }

        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        [Required]
        public string Password { get; set; }

        [StringLength(30, MinimumLength = 3), Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public DateTime? DateOfBirth { get; set; }
        
        public string Location { get; set; }
    }
}
