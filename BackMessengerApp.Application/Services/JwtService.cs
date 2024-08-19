using BackMessengerApp.Application.Interfaces;
using BackMessengerApp.Core.Models;
using BackMessengerApp.Core.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BackMessengerApp.Application.Services
{
	public class JwtService : IJwtService
	{
		private readonly IOptions<JwtSettings> _jwtSettings;

		public JwtService(IOptions<JwtSettings> jwtSettings)
		{
			_jwtSettings = jwtSettings;
		}

		public string GenerateAccessToken(User user, IEnumerable<string> roles)
		{
			var claims = new List<Claim>
			{
			    new Claim(ClaimTypes.NameIdentifier, user.Id),
				new Claim(JwtRegisteredClaimNames.Email, user.Email),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
			};

			foreach (var role in roles)
			{
				claims.Add(new Claim(ClaimTypes.Role, role));
			}

			int expireTime = int.Parse(_jwtSettings.Value.AccessExpireMinutes);

			return GenerateToken(claims, expireTime, _jwtSettings.Value.AccessKey);
		}

		public string GenerateRefreshToken(User user) {
			var claims = new[]
			{
				new Claim(JwtRegisteredClaimNames.Email, user.Email),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
			};

			int expireTime = int.Parse(_jwtSettings.Value.RefreshExpireMinutes);

			return GenerateToken(claims, expireTime, _jwtSettings.Value.RefreshKey);
		}

		private string GenerateToken(IEnumerable<Claim?> claims, int expireMinutes, string key)
		{
			var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
			var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

			var token = new JwtSecurityToken(
				issuer: _jwtSettings.Value.Issuer,
				audience: _jwtSettings.Value.Audience,
				claims: claims,
				expires: DateTime.Now.AddMinutes(expireMinutes),
				signingCredentials: credentials);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}

		public async Task<ClaimsIdentity?> ValidateRefreshToken(string refreshToken)
		{
			var tokenHandler = new JwtSecurityTokenHandler();
			var validationParameters = GetValidationRefreshParameters();

			//clear mapping for the correct types of claim instead of url
			tokenHandler.InboundClaimTypeMap.Clear();

			var result = await tokenHandler.ValidateTokenAsync(refreshToken, validationParameters);

			if (!result.IsValid) return null;

			return result.ClaimsIdentity;
		}

		private TokenValidationParameters GetValidationRefreshParameters()
		{
			return new TokenValidationParameters
			{
				ValidateIssuer = true,
				ValidateAudience = true,
				ValidateLifetime = true,
				ValidateIssuerSigningKey = true,
				ValidIssuer = _jwtSettings.Value.Issuer,	
				ValidAudience = _jwtSettings.Value.Audience,
				IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Value.RefreshKey))
			};
		}
	}
}
