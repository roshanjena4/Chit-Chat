using System.Diagnostics;
using System.Text;
using System.Text.Json;
using ChatApplication.Models;
using ChatApplication.Repositories;
using ChatApplication.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChatApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMessageRepository _repo;
        private readonly ILogger<HomeController> _logger;
        private readonly IAuthRepository _authRepository;
        private readonly AppDbContext _context;
        public HomeController(ILogger<HomeController> logger, IAuthRepository authRepository,
        IMessageRepository repo, AppDbContext context)
        {
            _authRepository = authRepository;
            _logger = logger;
            _context = context;
            _repo = repo;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Chat()
        {
            if(string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
            {
                return RedirectToAction("Login", "Authentication");
            }
            var Users = await _authRepository.FetchAllUser();
            return View(Users);
        }

        [HttpGet]
        public async Task<IActionResult> LoadHistory(int senderId, int receiverId)
        {
            var history = await _context.Messages
                .Where(m =>
                    (m.SenderId == senderId && m.ReceiverId == receiverId) ||
                    (m.SenderId == receiverId && m.ReceiverId == senderId))
                .OrderBy(m => m.CreatedAt)
                .Select(m => new {
                    m.SenderId,
                    m.ReceiverId,
                    m.MessageText,
                    createdAt = m.CreatedAt.ToString("o") 
                })
                .ToListAsync();

            return Json(history);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] SendDto dto)
        {
            using var connection = _repo.GetConnection();
            bool success = await _repo.SendMessageAsync(
                connection,
                dto.MessageText,
                dto.SenderId,
                dto.ReceiverId,
                $"user_{dto.ReceiverId}_queue"
            );

            if (success) return Json(new { success = true });
            return Json(new { success = false, error = "Could not send" });
        }

        [HttpPost]
        public IActionResult ReceiveMessage([FromBody] ReceiveMessageDto Model)
        {
            if (Model.ReceiverId == 0)
                return Json(null);
            var con = _repo.GetConnection();
            string queueName = $"user_{Model.ReceiverId}_queue";

            string messageJson = _repo.ReceiveMessage(con, queueName);
            if (messageJson == null)
                return Json(null);
            
            // Console.WriteLine("Message is json" + messageJson);

            var messageDto = JsonSerializer.Deserialize<EmailMessage>(messageJson);
            return Json(new {
                messageText = messageDto.MessageContent,
                senderId    = int.Parse(messageDto.SenderEmail),         
                createdAt   = DateTime.UtcNow.ToString("o")  
            });

        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Authentication");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}
