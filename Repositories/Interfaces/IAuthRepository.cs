using ChatApplication.Models;

namespace ChatApplication.Repositories.Interfaces
{
    public interface IAuthRepository
    {
        Task<List<User>> FetchAllUser();
        Task<User> Login(LoginModel user);
    }
}
