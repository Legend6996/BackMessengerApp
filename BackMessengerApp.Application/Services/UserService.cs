using BackMessengerApp.API.DTOs.User;
using BackMessengerApp.Application.DTOs.User;
using BackMessengerApp.Application.Interfaces;
using BackMessengerApp.Application.Results.Bot.Application.Results;
using BackMessengerApp.Core.Models;
using Microsoft.AspNetCore.Identity;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BackMessengerApp.Application.Services
{
	public class UserService : IUserService
	{
		private readonly UserManager<User> _userManager;
		private readonly IJwtService _jwtService;
		public UserService(UserManager<User> userManager, IJwtService jwtService)
		{
			_userManager = userManager;
			_jwtService = jwtService;
		}

		public async Task<ServiceResult<UserInfoDto>> GetUserInfo(ClaimsPrincipal claimsPrincipal)
		{
			var user = await _userManager.GetUserAsync(claimsPrincipal);
			if (user == null)
				return ServiceResult<UserInfoDto>.Fail("User not found");

			UserInfoDto userInfoDto = new()
			{
				Email = user.Email,
				UserName = user.UserName,
				Name = "",
			};

			return ServiceResult<UserInfoDto>.Success(userInfoDto);
		}

		public async Task<ServiceResult<Tokens>> LoginAsync(string email, string password)
		{
			var user = await _userManager.FindByEmailAsync(email);
			if (user == null)
				return ServiceResult<Tokens>.Fail("Invalid login or password");

			var result = await _userManager.CheckPasswordAsync(user, password);
			if (!result)
				return ServiceResult<Tokens>.Fail("Invalid login or password");

			var roles = await _userManager.GetRolesAsync(user);

			var token = _jwtService.GenerateAccessToken(user, roles);
			var refreshToken = _jwtService.GenerateRefreshToken(user);

			return ServiceResult<Tokens>.Success(new Tokens() { AccessToken = token, RefreshToken = refreshToken });

		}

		public async Task<ServiceResult<string>> RefreshAccessToken(string refreshToken)
		{
			var result = await _jwtService.ValidateRefreshToken(refreshToken);
			if (result == null) 
				return ServiceResult<string>.Fail("Not valid refresh token");

			var email = result.FindFirst(JwtRegisteredClaimNames.Email);
			if (email == null) 
				return ServiceResult<string>.Fail("Not valid refresh token");

			var user = await _userManager.FindByEmailAsync(email.Value);
			if (user == null)
				return ServiceResult<string>.Fail("Not valid refresh token");

			var roles = await _userManager.GetRolesAsync(user);

			var newAccessToken = _jwtService.GenerateAccessToken(user, roles);

			return ServiceResult<string>.Success(newAccessToken);
		}

		public async Task<ServiceResult<Tokens>> RegisterAsync(string name, string email, string password)
		{
			var existEmail = await _userManager.FindByEmailAsync(email);
			if (existEmail != null)
				return ServiceResult<Tokens>.Fail("Dublicate Email");

			User newUser = new()
			{
				UserName = name,
				Email = email,
				CreatedAt = DateTime.UtcNow,
			};

			var result = await _userManager.CreateAsync(newUser, password);
			if (!result.Succeeded)
			{
				if (result.Errors.FirstOrDefault().Code.Contains("DuplicateUserName"))
				{
					return ServiceResult<Tokens>.Fail($"Dublicate UserName");
				}
				return ServiceResult<Tokens>.Fail($"Failed to register {email}");
			}

			var roles = await _userManager.GetRolesAsync(newUser);

			var token = _jwtService.GenerateAccessToken(newUser, roles);
			var refreshToken = _jwtService.GenerateRefreshToken(newUser);

			return ServiceResult<Tokens>.Success(new Tokens() { AccessToken = token, RefreshToken = refreshToken });
		}
	}
}
