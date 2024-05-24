namespace UserService.Models;

public class UserModelDTO
{
    public UserModelDTO(UserModel userModel)
    {
        Id = userModel.Id;
        FirstName = userModel.FirstName;
        LastName = userModel.LastName;
        Email = userModel.Email;
        Username = userModel.Username;
        Address = userModel.Address;
        PhoneNumber = userModel.PhoneNumber;
        Type = UserModel.UserType.User;
        RegistrationDate = userModel.RegistrationDate;
    }

    public UserModelDTO()
    {
        
    }
    
    public string? Id {  get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Username { get; set; }
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public string? AuthToken { get; set; }
    public DateTime? RegistrationDate { get; set; }
    public UserModel.UserType Type = UserModel.UserType.User;
}