using BackMessengerApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BackMessengerApp.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/user")]
	public class UserController : ControllerBase
	{
		private readonly IUserService _userService;
		public UserController(IUserService userService)
		{
			_userService = userService;
		}

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
