using DatingApp.API.Models;

namespace DatingApp.API.Repository
{
    public interface IAuthRepository
    {
        Task<User> Register(User user, string password);
        Task<User> Login(string userName, string password);
        Task<bool> IsUserExists(string userName);
    }
}