using Document_Management.Data.Models;
using Document_Management.Services;
using Microsoft.AspNetCore.Mvc;

namespace Document_Management.Web.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestModel model)
        {
            var (status, message) = await _authService.Login(model);

            if (status == 0)
                return BadRequest(new { Message = message });
            return Ok(new { Token = message });

        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestModel model)
        {
            try
            {
                var (status, message) = await _authService.Register(model);

                if (status == 0)
                    return BadRequest(message);
                return Ok(new { Message = message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

    }
}
