using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RedMango_API.Data;
using RedMango_API.Models;
using RedMango_API.Models.DTO;
using RedMango_API.Utility;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace RedMango_API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		private readonly ApplicationDBContext _dbContext;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;
		private ApiResponse _response;
		private string _secret;
		public AuthController(IConfiguration configuration, ApplicationDBContext dBContext, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
		{
			_dbContext = dBContext;
			_userManager = userManager;
			_roleManager = roleManager;
			_response = new ApiResponse();
			_secret = configuration.GetValue<string>("ApiSetting:SecretKey");
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginRequestDTO loginRequestDTO)
		{
			ApplicationUser userFromDb = _dbContext.ApplicationUsers.FirstOrDefault(u => u.Email == loginRequestDTO.UserName);
			if (userFromDb == null)
			{
				_response.StatusCode = HttpStatusCode.Unauthorized;
				_response.ErrorList.Add("User doesnot Exists");
				_response.IsSuccess = false;
				return Unauthorized(_response);
			}

			var isValidUser = await _userManager.CheckPasswordAsync(userFromDb, loginRequestDTO.Password);
			if (!isValidUser)
			{
				_response.StatusCode = HttpStatusCode.Unauthorized;
				_response.ErrorList.Add("User Name or password is Incorrect");
				_response.IsSuccess = false;
				return Unauthorized(_response);
			}

			var roles = await _userManager.GetRolesAsync(userFromDb);

			// Generating JWT token
			byte[] key = Encoding.ASCII.GetBytes(_secret);
			JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
			SecurityTokenDescriptor securityTokenDescriptor = new SecurityTokenDescriptor()
			{
				Subject = new ClaimsIdentity(
					new Claim[]
					{
						new Claim(ClaimTypes.Name, userFromDb.Name),
						new Claim(ClaimTypes.Email, userFromDb.Email),
						new Claim("Id", userFromDb.Id.ToString()),
						new Claim(ClaimTypes.Role, roles.FirstOrDefault())
					}
				),
				Expires = DateTime.UtcNow.AddMinutes(5),
				SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
			};

			SecurityToken securityToken = tokenHandler.CreateToken(securityTokenDescriptor);
			LoginResponseDTO loginResponse = new()
			{
				Email = userFromDb.Email,
				Token = tokenHandler.WriteToken(securityToken),
			};

			if (string.IsNullOrEmpty(loginResponse.Token) || loginResponse.Email == null)
			{
				_response.StatusCode = HttpStatusCode.Unauthorized;
				_response.ErrorList.Add("User Name or password is Incorrect");
				_response.IsSuccess = false;
				return Unauthorized(_response);
			}

			_response.StatusCode = HttpStatusCode.OK;
			_response.Result = loginResponse;
			return Ok(_response);
		}

		[HttpPost("register")]
		public async Task<IActionResult> Register([FromBody] RegisterRequestDTO registerRequestDTO)
		{
			ApplicationUser userFromDb = _dbContext.ApplicationUsers.FirstOrDefault(u => u.UserName.ToLower() == registerRequestDTO.UserName.ToLower());

			if (userFromDb != null)
			{
				_response.StatusCode = HttpStatusCode.BadRequest;
				_response.ErrorList.Add("User Already Exists");
				_response.IsSuccess = false;
				return BadRequest(_response);
			}

			ApplicationUser newUser = new()
			{
				Name = registerRequestDTO.Name,
				Email = registerRequestDTO.UserName,
				UserName = registerRequestDTO.UserName,
				NormalizedUserName = registerRequestDTO.UserName.ToUpper(),
				NormalizedEmail = registerRequestDTO.UserName.ToUpper(),
			};

			try
			{
				IdentityResult result = await _userManager.CreateAsync(newUser, registerRequestDTO.Password);
				if (result.Succeeded)
				{
					if (!_roleManager.RoleExistsAsync(SD.ROLE_ADMIN).GetAwaiter().GetResult())
					{
						await _roleManager.CreateAsync(new IdentityRole(SD.ROLE_ADMIN));
					}
					if (!_roleManager.RoleExistsAsync(SD.ROLE_CUSTOMER).GetAwaiter().GetResult())
					{
						await _roleManager.CreateAsync(new IdentityRole(SD.ROLE_CUSTOMER));
					}

					if (registerRequestDTO.Role.ToLower() == SD.ROLE_ADMIN.ToLower())
					{
						await _userManager.AddToRoleAsync(newUser, SD.ROLE_ADMIN);
					}
					else
					{
						await _userManager.AddToRoleAsync(newUser, SD.ROLE_CUSTOMER);
					}

					_response.StatusCode = HttpStatusCode.OK;
					return Ok(_response);
				}
			}
			catch(Exception ex)
			{
				
			}
			_response.IsSuccess = false;
			_response.StatusCode = HttpStatusCode.BadGateway;
			return BadRequest(_response);
		}
	}
}
