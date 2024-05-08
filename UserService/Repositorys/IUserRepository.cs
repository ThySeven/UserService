using Microsoft.AspNetCore.Identity;
using UserService.Models;
namespace UserService.Repositorys
{
    public interface IUserRepository
    {
        IEnumerable<UserModel> GetAll();

        UserModel GetById(int id);

        void CreateUser(UserModel user);

        void DeleteUser(string id);

        UserModel UpdateUser(UserModel user);

        void ValidateUser(string userName, string password);
    }
}
