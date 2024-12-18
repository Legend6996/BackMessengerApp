using BackMessengerApp.Application.DTOs.Auth;
using BackMessengerApp.Application.Interfaces;
using BackMessengerApp.Application.Results.Bot.Application.Results;
using BackMessengerApp.Core.Models;
using BackMessengerApp.Core.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace BackMessengerApp.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IJwtService _jwtService;
        private readonly UserManager<User> _userManager;
        private readonly IOptions<GoogleAuthSettings> _googleAuthSettings;
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthService(IJwtService jwtService, UserManager<User> userManager, IOptions<GoogleAuthSettings> googleAuthSettings, IHttpClientFactory httpClientFactory)
        {
            _jwtService = jwtService;
            _userManager = userManager;
            _googleAuthSettings = googleAuthSettings;
            _httpClientFactory = httpClientFactory;
        }

        public string GetGoogleRedirectLink()
        {
            var clientId = _googleAuthSettings.Value.ClientId;
            var redirectUri = _googleAuthSettings.Value.RedirectUrl;
            var scope = "openid profile email";
            var responseType = "code";
            var googleAuthUrl = $"https://accounts.google.com/o/oauth2/v2/auth?client_id={clientId}&redirect_uri={redirectUri}&response_type={responseType}&scope={scope}";
            return googleAuthUrl;
        }

        public async Task<ServiceResult<JwtTokens>> LoginAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return ServiceResult<JwtTokens>.Fail("Invalid login or password");

            var result = await _userManager.CheckPasswordAsync(user, password);
            if (!result)
                return ServiceResult<JwtTokens>.Fail("Invalid login or password");

            var jwtTokens = await GetTokensAsync(user);

            return ServiceResult<JwtTokens>.Success(jwtTokens);

        }

        public async Task<ServiceResult<JwtTokens>> LoginWithGoogleAsync(string googleCode)
        {
            var accessToken = await GetGoogleTokenAsync(googleCode);
            if(accessToken == null)
                return ServiceResult<JwtTokens>.Fail("Failed to get Google token.");

            var userInfo = await GetGoogleUserInfoAsync(accessToken);
            if (userInfo == null)
                return ServiceResult<JwtTokens>.Fail("Failed to get Google user info.");

            var existUser = await _userManager.FindByEmailAsync(userInfo.Email);

            JwtTokens jwtTokens = new();

            if(existUser != null)
                jwtTokens = await GetTokensAsync(existUser);
            else
                return await CreateUserAsync(userInfo.Name, userInfo.Email, userInfo.Email);

            return ServiceResult<JwtTokens>.Success(jwtTokens);
        }

        public async Task<ServiceResult<string>> RefreshAccessToken(string refreshToken)
        {
            var result = await _jwtService.ValidateRefreshToken(refreshToken);
            if (result == null)
                return ServiceResult<string>.Fail("Not valid refresh token");

            var email = result.Claims.First(claim => claim.Type == "email");
            if (email == null)
                return ServiceResult<string>.Fail("Not valid refresh token");

            var user = await _userManager.FindByEmailAsync(email.Value);
            if (user == null)
                return ServiceResult<string>.Fail("Not valid refresh token");

            var roles = await _userManager.GetRolesAsync(user);

            var newAccessToken = _jwtService.GenerateAccessToken(user, roles);

            return ServiceResult<string>.Success(newAccessToken);
        }

        public async Task<ServiceResult<JwtTokens>> RegisterAsync(string name, string userName, string email, string password)
        {
            var existEmail = await _userManager.FindByEmailAsync(email);
            if (existEmail != null)
                return ServiceResult<JwtTokens>.Fail("Dublicate Email");

            return await CreateUserAsync(name, userName, email, password);
        }


        private async Task<string?> GetGoogleTokenAsync(string googleCode)
        {
            var clientId = _googleAuthSettings.Value.ClientId;
            var clientSecret = _googleAuthSettings.Value.ClientSecret;
            var redirectUri = _googleAuthSettings.Value.RedirectUrl;
            var tokenEndpoint = "https://oauth2.googleapis.com/token";

            var client = _httpClientFactory.CreateClient();
            var requestContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("code", googleCode),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("redirect_uri", redirectUri),
                new KeyValuePair<string, string>("grant_type", "authorization_code")
            });

            var response = await client.PostAsync(tokenEndpoint, requestContent);
            if (!response.IsSuccessStatusCode)
                return null;

            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
            
            return tokenResponse.GetProperty("access_token").GetString();
        }

        private async Task<GoogleUserInfo?> GetGoogleUserInfoAsync(string accessToken)
        {
            var userInfoEndpoint = "https://www.googleapis.com/oauth2/v1/userinfo?alt=json";
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var userInfoResponse = await client.GetAsync(userInfoEndpoint);
            if (!userInfoResponse.IsSuccessStatusCode)
                return null;

            var userInfoContent = await userInfoResponse.Content.ReadAsStringAsync();
            var userInfo = JsonSerializer.Deserialize<GoogleUserInfo>(userInfoContent);
            return userInfo;
        }


        private async Task<ServiceResult<JwtTokens>> CreateUserAsync(string name, string email, string userName, string? password = null)
        {
            User newUser = new()
            {
                Name = name,
                UserName = userName,
                Email = email,
                CreatedAt = DateTime.UtcNow,
            };

            IdentityResult result;
            if (password != null)
                result = await _userManager.CreateAsync(newUser, password);
            else
                result = await _userManager.CreateAsync(newUser);

            if (!result.Succeeded)
            {
                if (result.Errors.FirstOrDefault().Code.Contains("DuplicateUserName"))
                {
                    return ServiceResult<JwtTokens>.Fail($"Dublicate UserName");
                }
                return ServiceResult<JwtTokens>.Fail($"Failed to register {email}");
            }

            var jwtTokens = await GetTokensAsync(newUser);

            return ServiceResult<JwtTokens>.Success( jwtTokens );
        }

        private async Task<JwtTokens> GetTokensAsync(User user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var token = _jwtService.GenerateAccessToken(user, roles);
            var refreshToken = _jwtService.GenerateRefreshToken(user);
            return new JwtTokens() { AccessToken = token, RefreshToken = refreshToken };
        }
    }
}
