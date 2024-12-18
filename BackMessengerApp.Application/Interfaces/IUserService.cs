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
		Task<ServiceResult<UserInfoDto>> GetUserInfo(ClaimsPrincipal claimsPrincipal);
	}
}
