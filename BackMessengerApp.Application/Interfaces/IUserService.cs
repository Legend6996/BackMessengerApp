using BackMessengerApp.API.DTOs.User;
using BackMessengerApp.Application.DTOs.User;
using BackMessengerApp.Application.Results.Bot.Application.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BackMessengerApp.Application.Interfaces
{
	public interface IUserService
	{
		Task<ServiceResult<Tokens>> RegisterAsync(string name, string email, string password);
		Task<ServiceResult<Tokens>> LoginAsync(string email, string password);
		Task<ServiceResult<string>> RefreshAccessToken(string refreshToken);
		Task<ServiceResult<UserInfoDto>> GetUserInfo(ClaimsPrincipal claimsPrincipal);
	}
}
