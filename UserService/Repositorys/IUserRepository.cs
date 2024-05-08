using Microsoft.AspNetCore.Identity;
using UserService.Models;
namespace UserService.Repositorys
{
    public interface IUserRepository
    {
        IEnumerable<ÚserModel> GetAll();

        ÚserModel GetById(int id);

        void CreateUser(ÚserModel úser);

        void DeleteUser(string id);

        void UpdateUser(ÚserModel úser);

        void ValidateUser(string userName, string password);
    }
}
