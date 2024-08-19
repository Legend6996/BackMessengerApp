using BackMessengerApp.API.DTOs.User;
using BackMessengerApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BackMessengerApp.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class UserController : ControllerBase
	{
		private readonly IUserService _userService;
		public UserController(IUserService userService)
		{
			_userService = userService;
		}


		[HttpPost("login")]
		public async Task<IActionResult> Login(LoginRequest request)
		{
			var result = await _userService.LoginAsync(request.Email, request.Password);

			if(!result.IsSuccessful)
				return BadRequest(new { text = result.Errors.FirstOrDefault() });

			return Ok(result.Data);
		}

		[HttpPost("register")]
		public async Task<IActionResult> Register(RegisterRequest request)
		{
			if(!ModelState.IsValid)
				return BadRequest(ModelState);

			var result = await _userService.RegisterAsync(request.Name, request.Email, request.Password);

			if(!result.IsSuccessful)
				return BadRequest(new { text = result.Errors.FirstOrDefault() });

			return Ok(result.Data);
		}


		[HttpPost("refresh")]
		public async Task<IActionResult> RefreshAccessToken(RefreshTokenRequest request)
		{
			var result = await _userService.RefreshAccessToken(request.RefreshToken);

			if(!result.IsSuccessful)
				return Unauthorized(new { text = result.Errors.FirstOrDefault() });

			return Ok(new { accessToken = result.Data });
		}

		[Authorize]
		[HttpGet("info")]
		public async Task<IActionResult> UserInfo()
		{
			var claimsPrincipal = HttpContext.User;

			var result = await _userService.GetUserInfo(claimsPrincipal);
			if(!result.IsSuccessful)
				return BadRequest(new { text = result.Errors.FirstOrDefault() });

			return Ok(result.Data);
		}
	}
}
