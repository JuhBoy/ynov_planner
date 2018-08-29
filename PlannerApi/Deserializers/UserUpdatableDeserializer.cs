using System;
using System.ComponentModel.DataAnnotations;
using events_planner.Models;

namespace events_planner.Deserializers {
    public class UserUpdatableDeserializer {

        [StringLength(20, MinimumLength = 3)]
        [MaxLength(20, ErrorMessage = "First Name must be under 20 characters")]
        [MinLength(3, ErrorMessage = "First Name must be at least 3 characters")]
        public string FirstName { get; set; }


        [StringLength(20, MinimumLength = 3)]
        [MaxLength(20, ErrorMessage = "Last Name must be under 20 characters")]
        [MinLength(3, ErrorMessage = "Last Name must be at least 3 characters")]
        public string LastName { get; set; }

        [EmailAddress]
        [StringLength(30, MinimumLength = 3)]
        public string Email { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        public string Password { get; set; }

        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        public string PasswordConfirmation { get; set; }

        [Phone]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        public string PhoneNumber { get; set; }

        [Url]
        public string ImageUrl { get; set; }

        public string Location { get; set; }

        public void BindWithUser<T>(ref T user) where T: User {
            if (Password != null && (Password != PasswordConfirmation)) {
                throw new ValidationException("Confirmed password is not valid");
            }

            if (LastName != null)
                user.LastName = LastName;
            if (FirstName != null)
                user.FirstName = FirstName;
            if (Email != null)
                user.Email = Email;
            if (Password != null)
                user.Password = Password;
            if (PhoneNumber != null)
                user.PhoneNumber = int.Parse(PhoneNumber);
            if (ImageUrl != null)
                user.ImageUrl = ImageUrl;
            if (Location != null)
                user.Location = Location;
            if (DateOfBirth.HasValue)
                user.DateOfBirth = DateOfBirth;
        }
    }
}