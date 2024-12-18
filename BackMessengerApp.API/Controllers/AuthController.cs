using BackMessengerApp.API.DTOs.Auth;
using BackMessengerApp.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BackMessengerApp.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet("login-google")]
        public IActionResult LoginGoogle()
        {
            string googleLink = _authService.GetGoogleRedirectLink();

            return Redirect(googleLink);
        }

        [HttpPost("login-with-google")]
        public async Task<IActionResult> LoginWithGoogle([FromBody] string code)
        {
            var result = await _authService.LoginWithGoogleAsync(code);

            if(!result.IsSuccessful) 
                return BadRequest(new { text = result.Errors.FirstOrDefault() });

            return Ok(result.Data);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var result = await _authService.LoginAsync(request.Email, request.Password);

            if (!result.IsSuccessful)
                return BadRequest(new { text = result.Errors.FirstOrDefault() });

            return Ok(result.Data);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RegisterAsync(request.Name, request.UserName, request.Email, request.Password);

            if (!result.IsSuccessful)
                return BadRequest(new { text = result.Errors.FirstOrDefault() });

            return Ok(result.Data);
        }


        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshAccessToken(RefreshTokenRequest request)
        {
            var result = await _authService.RefreshAccessToken(request.RefreshToken);

            if (!result.IsSuccessful)
                return Unauthorized(new { text = result.Errors.FirstOrDefault() });

            return Ok(new { accessToken = result.Data });
        }
    }
}
