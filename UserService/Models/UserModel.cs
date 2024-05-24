namespace UserService.Models
{
    public class UserModel
    { 
        public string? Id {  get; set; } = Guid.NewGuid().ToString();
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string? Salt { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public bool Verified { get; set; } = false;
        public UserType Type = UserType.User;
        public enum UserType
        {
            User, Admin
        }
    }
}
