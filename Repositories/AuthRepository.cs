using ChatApplication.Models;
using ChatApplication.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChatApplication.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor; 

        public AuthRepository(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor; 
        }

        public async Task<List<User>> FetchAllUser()
        {
            var usermail = _httpContextAccessor.HttpContext?.Session.GetString("Username"); 
            return await _context.Users.Where(x => x.Email != usermail).ToListAsync(); 
        }

        public async Task<User> Login(LoginModel user)
        {
            var userDetail = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == user.Email && u.Password == user.Password);
            return userDetail;
        }

    }
}
