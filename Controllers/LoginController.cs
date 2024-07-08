using auth_jwt.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace auth_jwt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private IConfiguration _config;
        public LoginController(IConfiguration configuration)
        {
            _config = configuration;
        }
        private User Authentication(User user)
        {
            User _user = null;
            if (user.Username == "admin" && user.Password == "123456")
            {
                _user = new User { Username = "dihfodhfh" };
            }
            return _user;
        }
        private string GenerateToken(User user)
        {
            var securitykey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securitykey, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(_config["Jwt:Issuer"], _config["Jwt:Audience"], null,
                expires: DateTime.Now.AddMinutes(1),
                signingCredentials: credentials
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login(User user)
        {
            IActionResult response = Unauthorized();
            var user_ = AuthenticateUser(user);
            if(user_ != null) 
            {
                var token = GenerateToken(user_);
                response = Ok(new { token = token });
        }
            return response;
    }
        private User AuthenticateUser(User loginCredentials)
        {
            // Example logic to authenticate user
            // Replace this with your actual authentication logic
            if (loginCredentials.Username == "admin" && loginCredentials.Password == "123456")
            {
                return new User { Username = loginCredentials.Username, Password = loginCredentials.Password };
            }
            return null;
        }


    }
}
