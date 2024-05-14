using Microsoft.AspNetCore.Identity;
using UserService.Models;
namespace UserService.Repositories
{
    public interface IUserRepository
    {
        //IEnumerable<UserModelDTO> GetAll();

        UserModelDTO GetById(string id);

        void CreateUser(UserModel user);

        void DeleteUser(string id);

        void VerifyUser(string id);

        UserModelDTO UpdateUser(UserModelDTO newUserData);
        
        void UpdatePassword(LoginModel credentials, string newPassword);

        UserModelDTO Login(LoginModel credentials);
    }
}
