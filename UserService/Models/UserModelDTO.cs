namespace UserService.Models;

public class UserModelDTO
{
    public UserModelDTO(UserModel userModel)
    {
        id = userModel.id;
        firstName = userModel.firstName;
        lastName = userModel.lastName;
        email = userModel.email;
        userName = userModel.userName;
        address = userModel.address;
        phoneNumber = userModel.phoneNumber;
        verified = userModel.verified;
    }
    
    public string id {  get; set; }
    public string firstName { get; set; }
    public string lastName { get; set; }
    public string email { get; set; }
    public string userName { get; set; }
    public string address { get; set; }
    public string phoneNumber { get; set; }
    public bool verified { get; set; } = false;
}