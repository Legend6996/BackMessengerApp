using BackMessengerApp.Core.Models;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BackMessengerApp.Application.Interfaces
{
	public interface IJwtService
	{
		string GenerateAccessToken(User user, IEnumerable<string> roles);
		string GenerateRefreshToken(User user);
		Task<ClaimsIdentity?> ValidateRefreshToken(string refreshToken);
	}
}
