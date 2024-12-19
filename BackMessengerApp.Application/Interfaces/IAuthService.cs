using BackMessengerApp.Application.DTOs.Auth;
using BackMessengerApp.Application.Enums;
using BackMessengerApp.Application.Results.Bot.Application.Results;

namespace BackMessengerApp.Application.Interfaces
{
    public interface IAuthService
    {
        Task<ServiceResult<JwtTokens>> RegisterAsync(string name, string userName, string email, string password);
        Task<ServiceResult<JwtTokens>> LoginAsync(string email, string password);
        Task<ServiceResult<JwtTokens>> LoginOAuthAsync(OAuthProvider provider, string code);
        string GetOAuthRedirectLink(OAuthProvider provider);
        Task<ServiceResult<string>> RefreshAccessToken(string refreshToken);
    }
}
