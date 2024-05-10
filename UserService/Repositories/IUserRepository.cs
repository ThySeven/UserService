using Microsoft.AspNetCore.Identity;
using UserService.Models;
namespace UserService.Repositories
{
    public interface IUserRepository
    {
        IEnumerable<UserModel> GetAll();

        UserModel GetById(string id);

        void CreateUser(UserModel user);

        void DeleteUser(string id);

        void VerifyUser(string id);

        UserModel UpdateUser(UserModel newUserData);

        void ValidateUser(string userName, string password);
    }
}
