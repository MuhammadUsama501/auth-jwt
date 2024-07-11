//using auth_jwt.Model;
//using auth_jwt.Models;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Configuration;
//using Microsoft.IdentityModel.Tokens;
//using System.IdentityModel.Tokens.Jwt;
//using System.Text;

//namespace auth_jwt.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class LoginController : ControllerBase
//    {
//        private readonly IConfiguration _config;
//        private readonly AppDbContext _context;

//        public LoginController(IConfiguration configuration, AppDbContext context)
//        {
//            _config = configuration;
//            _context = context;

//        }
//        private User? AuthenticateUser(User user)
//        {
//            // Replace this with your actual authentication logic
//            var authenticatedUser = _context.Users
//                .FirstOrDefault(u => u.Username == user.Username && u.Password == user.Password);

//            return authenticatedUser;
//        }

//        private string GenerateToken(User user)
//        {
//            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
//            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

//            var token = new JwtSecurityToken(
//                issuer: _config["Jwt:Issuer"],
//                audience: _config["Jwt:Audience"],
//                expires: DateTime.Now.AddMinutes(30),
//                signingCredentials: credentials
//            );

//            return new JwtSecurityTokenHandler().WriteToken(token);
//        }

//        [AllowAnonymous]
//        [HttpPost("login")]
//        public IActionResult Login([FromBody] User user)
//        {
//            IActionResult response = Unauthorized();
//            var authenticatedUser = AuthenticateUser(user);

//            if (authenticatedUser != null)
//            {
//                var token = GenerateToken(authenticatedUser);
//                response = Ok(new { token });
//            }

//            return response;
//        }
//    }
//}
using auth_jwt.Model;
using auth_jwt.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace auth_jwt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly AppDbContext _context;

        public LoginController(IConfiguration configuration, AppDbContext context)
        {
            _config = configuration;
            _context = context;
        }

        private User? AuthenticateUser(User user)
        {
            return _context.Users.FirstOrDefault(u => u.Username == user.Username && u.Password == user.Password);
        }

        private string GenerateToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login([FromBody] User user)
        {
            IActionResult response = Unauthorized();
            var authenticatedUser = AuthenticateUser(user);

            if (authenticatedUser != null)
            {
                var token = GenerateToken(authenticatedUser);
                response = Ok(new { token });
            }

            return response;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            // Check if the username already exists
            if (_context.Users.Any(u => u.Username == user.Username))
            {
                return BadRequest("Username already exists.");
            }

            // Hash the password before saving
            user.Password = HashPassword(user.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("User registered successfully.");
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                var builder = new StringBuilder();
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}

