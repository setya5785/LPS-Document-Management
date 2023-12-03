using System.ComponentModel.DataAnnotations;

namespace Document_Management.Data.Models
{
    public class LoginRequestModel
    {
        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; }
        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }
    }
}
