using System.ComponentModel.DataAnnotations;

namespace ChatApplication.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string? ProfilePicture { get; set; }
    }
    public class LoginModel
    {
        [Required]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
    public class SignupModel
    {
        public string Name { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }
    }
    public class ReceiveMessageDto
    {
        public int ReceiverId { get; set; }
    }



}
