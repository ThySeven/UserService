namespace UserService.Models;

public class UserModelDTO
{
    public UserModelDTO(UserModel userModel)
    {
        Id = userModel.Id;
        FirstName = userModel.FirstName;
        LastName = userModel.LastName;
        Email = userModel.Email;
        UserName = userModel.UserName;
        Address = userModel.Address;
        PhoneNumber = userModel.PhoneNumber;
    }

    public UserModelDTO()
    {
        
    }
    
    public string Id {  get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? UserName { get; set; }
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
}