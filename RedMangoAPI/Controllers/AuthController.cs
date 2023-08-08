using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RedMangoAPI.Data;
using RedMangoAPI.Models;
using RedMangoAPI.Models.DTO;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text;
using System.Security.Claims;

namespace RedMangoAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDBContext _db;
        private APIResponse _response;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private string secretKey;
        public AuthController(ApplicationDBContext db, IConfiguration configuration, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _response = new APIResponse();
            secretKey = configuration.GetValue<string>("ApiSettings:Secret");
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO  model)
        {
            ApplicationUser userFromDb = _db.ApplicationUsers.FirstOrDefault(u => u.UserName.ToLower() ==  model.UserName.ToLower());

            bool isValid = await _userManager.CheckPasswordAsync(userFromDb, model.Password);
            if (!isValid)
            {
                _response.Result = new LoginRequestDTO();
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Username or password is incorrect");
                return BadRequest(_response);
            }

            var roles = await _userManager.GetRolesAsync(userFromDb);
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            byte[] key = Encoding.ASCII.GetBytes(secretKey);

            SecurityTokenDescriptor tokenDescriptor = new()
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("fullName", userFromDb.Name),
                    new Claim("id", userFromDb.Id.ToString()),
                    new Claim(ClaimTypes.Email, userFromDb.UserName.ToString()),
                    new Claim(ClaimTypes.Role, roles.FirstOrDefault()),
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

            LoginResponseDTO loginResponse = new LoginResponseDTO()
            {
                Email = userFromDb.Email,
                Token = tokenHandler.WriteToken(token)
            };

            if (loginResponse.Email == null || string.IsNullOrEmpty(loginResponse.Token))
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Username or password is incorrect");
                return BadRequest(_response);
            }

            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Result = loginResponse;
            return Ok(_response);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO model)
        {
            ApplicationUser userFromDb = _db.ApplicationUsers.FirstOrDefault(u=>u.UserName.ToLower() == model.UserName.ToLower());

            if (userFromDb != null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Username already exists");
                return BadRequest(_response);
            }

            ApplicationUser newUser = new()
            {
                UserName = model.UserName,
                Email = model.UserName,
                NormalizedEmail = model.UserName.ToUpper(),
                Name = model.Name,
            };

            try
            {
                var result = await _userManager.CreateAsync(newUser, model.Password);
                if (result.Succeeded)
                {
                    if (!_roleManager.RoleExistsAsync(UserRole.Role_Admin).GetAwaiter().GetResult())
                    {
                        //create roles in database
                        await _roleManager.CreateAsync(new IdentityRole(UserRole.Role_Admin));
                        await _roleManager.CreateAsync(new IdentityRole(UserRole.Role_Customer));
                    }
                    if (model.Role.ToLower() == UserRole.Role_Admin)
                    {
                        await _userManager.AddToRoleAsync(newUser, UserRole.Role_Admin);
                    }
                    else
                    {
                        await _userManager.AddToRoleAsync(newUser, UserRole.Role_Customer);
                    }

                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    return Ok(_response);
                }
            }
            catch (Exception)
            {

            }
            _response.StatusCode = HttpStatusCode.BadRequest;
            _response.IsSuccess = false;
            _response.ErrorMessages.Add("Error while registering");
            return BadRequest(_response);
        }
    }
}
