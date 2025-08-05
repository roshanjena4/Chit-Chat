using Microsoft.AspNetCore.Mvc;
using ChatApplication.Models;
using ChatApplication.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChatApplication.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly IAuthRepository _authRepository;
        private readonly AppDbContext _context ;


        public AuthenticationController(IAuthRepository authRepository, AppDbContext context)
        {
            _authRepository = authRepository;
            _context = context;
        }
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
            

            var user = await _authRepository.Login(model);

            if (user == null)
            {
                ModelState.AddModelError(model.Email, "Invalid username or password");
                return View(model);
            }

            // Save login session 
            HttpContext.Session.SetString("Username", user.Email);
            HttpContext.Session.SetString("Name", user.Name);
            HttpContext.Session.SetString("UserId", user.Id.ToString());

            return RedirectToAction("Chat", "Home");
        } 
        public IActionResult Signup()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Signup(SignupModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if email already exists
            var exists = await _context.Users.AnyAsync(u => u.Email == model.Email);
            if (exists)
            {
                ModelState.AddModelError("", "Email already registered.");
                return View(model);
            }

            var user = new User
            {
                Name = model.Name,
                Email = model.Email,
                Password = model.Password 
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Login");
        }
    }
}
