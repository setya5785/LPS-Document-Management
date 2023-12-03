using Document_Management.Data.Models;
using Document_Management.Domain;
using System.Security.Claims;

namespace Document_Management.Services
{
    public interface IAuthService
    {
        Task<(int, string)> Login(LoginRequestModel model);
        Task<(int, string)> Register(RegisterRequestModel model);
    }
}
