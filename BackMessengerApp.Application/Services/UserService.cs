using BackMessengerApp.Application.DTOs.User;
using BackMessengerApp.Application.Interfaces;
using BackMessengerApp.Application.Results.Bot.Application.Results;
using BackMessengerApp.Core.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace BackMessengerApp.Application.Services
{
	public class UserService : IUserService
	{
		private readonly UserManager<User> _userManager;
		public UserService(UserManager<User> userManager)
		{
			_userManager = userManager;
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
				Name = user.Name,
			};

			return ServiceResult<UserInfoDto>.Success(userInfoDto);
		}
	}
}
