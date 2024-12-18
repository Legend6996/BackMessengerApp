using BackMessengerApp.Application.DTOs.Auth;
using BackMessengerApp.Application.Results.Bot.Application.Results;

namespace BackMessengerApp.Application.Interfaces
{
    public interface IAuthService
    {
        Task<ServiceResult<JwtTokens>> RegisterAsync(string name, string userName, string email, string password);
        Task<ServiceResult<JwtTokens>> LoginAsync(string email, string password);
        Task<ServiceResult<JwtTokens>> LoginWithGoogleAsync(string googleCode);
        string GetGoogleRedirectLink();
        Task<ServiceResult<string>> RefreshAccessToken(string refreshToken);
    }
}
