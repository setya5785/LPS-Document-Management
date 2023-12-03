using Document_Management.Data;
using Document_Management.Data.Models;
using Document_Management.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Document_Management.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public AuthService(ApplicationDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }

        public async Task<(int, string)> Login(LoginRequestModel model)
        {
            var userExist = await _dbContext.Users.FirstOrDefaultAsync(c => c.Username == model.Username);
            if (userExist == null)
                return (0, "Invalid username");
            if (!ValidatePassword(model.Password, userExist.Password))
            {
                return (0, "Invalid password");
            }

            var authClaims = new List<Claim>
            {
               new Claim(ClaimTypes.Name, userExist.Username),
               new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
               new Claim(ClaimTypes.Role, userExist.UserRole),
               new Claim("UserId", userExist.Id.ToString())
            };

            string token = GenerateToken(authClaims);
            return (1, token);
        }


        private bool ValidatePassword(string enteredPassword, string storedPassword)
        {
            return HashPassword(enteredPassword) == storedPassword;
        }

        public async Task<(int, string)> Register(RegisterRequestModel model)
        {
            var userExist = await _dbContext.Users.FirstOrDefaultAsync(c => c.Username == model.Username);
            if (userExist != null)
            {
                return (0, "User already exists");
            }                

            // Check if password valid
            if (!IsValidPassword(model.Password))
            {
                throw new InvalidOperationException("Password does not meet the requirements.");
            }

            var hashedPassword = HashPassword(model.Password);

            User user = new User
            {
                Username = model.Username,
                Password = hashedPassword,
                UserRole = model.UserRole,
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            return (1, "User created successfully!");
        }

        public bool IsValidPassword(string password)
        {
            // Check if it contains at least one lowercase English character
            if (!password.Any(char.IsLower))
                return false;

            // Check if it contains at least one uppercase English character
            if (!password.Any(char.IsUpper))
                return false;

            // Check if it contains at least one special character
            if (!password.Any(IsSpecialCharacter))
                return false;

            // Check if its length is at least 8
            if (password.Length < 8)
                return false;

            // Check if it contains at least one digit
            if (!password.Any(char.IsDigit))
                return false;

            // If all checks pass, the password is valid
            return true;
        }

        private bool IsSpecialCharacter(char c)
        {
            // Define the allowed special characters
            char[] specialCharacters = { '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '-', '+' };

            // Check if the character is one of the allowed special characters
            return specialCharacters.Contains(c);
        }

        private string HashPassword(string password)
        {
            // hash this password for storing
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        private string GenerateToken(IEnumerable<Claim> claims)
        {
            //generate token with 30 mins expiration
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWTKey:Secret"]));
            var _TokenExpiryTimeInHour = Convert.ToInt64(_configuration["JWTKey:TokenExpiryTimeInHour"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = _configuration["JWTKey:ValidIssuer"],
                Audience = _configuration["JWTKey:ValidAudience"],
                //Expires = DateTime.UtcNow.AddHours(_TokenExpiryTimeInHour),
                Expires = DateTime.UtcNow.AddMinutes(30),
                SigningCredentials = new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256),
                Subject = new ClaimsIdentity(claims)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
