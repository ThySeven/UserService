using System.ComponentModel.DataAnnotations;

namespace UserService.Models
{
    public class UserModelInteropablility
    {
        [Required]
        [Key]
        public Guid Id { get; set; } // UUIDs are represented as Guid in C#

        [Required]
        [MinLength(1)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public DateTime RegistrationDate { get; set; }
    }

    public static class UserConverter
    {
        public static UserModelInteropablility ToInteropablility(UserModelDTO userModel)
        {
            if (userModel == null)
            {
                throw new ArgumentNullException(nameof(userModel));
            }

            // Ensure non-nullable properties are filled
            if (string.IsNullOrWhiteSpace(userModel.Username))
            {
                throw new ArgumentException("Username is required.", nameof(userModel.Username));
            }

            if (string.IsNullOrWhiteSpace(userModel.Email))
            {
                throw new ArgumentException("Email is required.", nameof(userModel.Email));
            }

            if (!userModel.RegistrationDate.HasValue)
            {
                throw new ArgumentException("RegistrationDate is required.", nameof(userModel.RegistrationDate));
            }

            return new UserModelInteropablility
            {
                Id = Guid.Parse(userModel.Id),
                Username = userModel.Username,
                Email = userModel.Email,
                RegistrationDate = userModel.RegistrationDate.Value
            };
        }
    }

}
