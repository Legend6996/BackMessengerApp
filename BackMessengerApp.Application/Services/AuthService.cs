using BackMessengerApp.Application.DTOs.Auth;
using BackMessengerApp.Application.Enums;
using BackMessengerApp.Application.Interfaces;
using BackMessengerApp.Application.Results.Bot.Application.Results;
using BackMessengerApp.Core.Models;
using BackMessengerApp.Core.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace BackMessengerApp.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IJwtService _jwtService;
        private readonly UserManager<User> _userManager;
        private readonly IOptions<GoogleOAuthSettings> _googleOAuthSettings;
        private readonly IOptions<YandexOAuthSettings> _yandexOAuthSettings;
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthService(IJwtService jwtService, UserManager<User> userManager, IOptions<GoogleOAuthSettings> googleOAuthSettings, IHttpClientFactory httpClientFactory, IOptions<YandexOAuthSettings> yandexOAuthSettings)
        {
            _jwtService = jwtService;
            _userManager = userManager;
            _googleOAuthSettings = googleOAuthSettings;
            _httpClientFactory = httpClientFactory;
            _yandexOAuthSettings = yandexOAuthSettings;
        }

        public string GetOAuthRedirectLink(OAuthProvider provider)
        {
            var settings = GetOAuthSettings(provider);
            string scope;
            string responseType = "code";
            string authUrl;

            switch (provider)
            {
                case OAuthProvider.Google:
                    scope = "openid profile email";
                    authUrl = $"https://accounts.google.com/o/oauth2/v2/auth?client_id={settings.ClientId}&redirect_uri={settings.RedirectUrl}&response_type={responseType}&scope={scope}";
                    break;
                case OAuthProvider.Yandex:
                    scope = "login:email login:info";
                    authUrl = $"https://oauth.yandex.ru/authorize?client_id={settings.ClientId}&redirect_uri={settings.RedirectUrl}&response_type={responseType}&scope={scope}";
                    break;
                default:
                    authUrl = "";
                    break;
            }

            return authUrl;
        }

        public async Task<ServiceResult<JwtTokens>> LoginAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return ServiceResult<JwtTokens>.Fail("Invalid login or password");

            var result = await _userManager.CheckPasswordAsync(user, password);
            if (!result)
                return ServiceResult<JwtTokens>.Fail("Invalid login or password");

            var jwtTokens = await GetJwtTokensAsync(user);

            return ServiceResult<JwtTokens>.Success(jwtTokens);
        }

        public async Task<ServiceResult<JwtTokens>> LoginOAuthAsync(OAuthProvider provider, string code)
        {
            var accessToken = await GetOAuthAccessTokenAsync(provider, code);
            if(accessToken == null)
                return ServiceResult<JwtTokens>.Fail("Failed to get OAuth token.");

            var userInfo = await GetOAuthUserInfoAsync(provider, accessToken);
            if (userInfo == null)
                return ServiceResult<JwtTokens>.Fail("Failed to get OAuth user info.");

            var existUser = await _userManager.FindByEmailAsync(userInfo.Email);

            JwtTokens jwtTokens = new();

            if(existUser != null)
                jwtTokens = await GetJwtTokensAsync(existUser);
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

        private async Task<string?> GetOAuthAccessTokenAsync(OAuthProvider provider, string code)
        {
            var settings = GetOAuthSettings(provider);
            string tokenEndpoint = "";

            var parameters = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("redirect_uri", settings.RedirectUrl),
                new KeyValuePair<string, string>("grant_type", "authorization_code")
            };

            var client = _httpClientFactory.CreateClient();

            switch (provider)
            {
                case OAuthProvider.Google:
                    parameters.AddRange(new[]
                    {
                        new KeyValuePair<string, string>("client_id", settings.ClientId),
                        new KeyValuePair<string, string>("client_secret", settings.ClientSecret),
                    });
                    tokenEndpoint = "https://oauth2.googleapis.com/token";
                    break;
                case OAuthProvider.Yandex:
                    var byteArray = Encoding.ASCII.GetBytes($"{settings.ClientId}:{settings.ClientSecret}");
                    var base64String = Convert.ToBase64String(byteArray);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64String);
                    tokenEndpoint = "https://oauth.yandex.ru/token";
                    break;
            }

            var requestFormContent = new FormUrlEncodedContent(parameters);

            var response = await client.PostAsync(tokenEndpoint, requestFormContent);
            if (!response.IsSuccessStatusCode)
                return null;

            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

            return tokenResponse.GetProperty("access_token").GetString();
        }

        private OAuthSettings GetOAuthSettings(OAuthProvider provider)
        {
            switch (provider)
            {
                case OAuthProvider.Google:
                    return _googleOAuthSettings.Value;
                case OAuthProvider.Yandex:
                    return _yandexOAuthSettings.Value;
                default:
                    throw new ArgumentOutOfRangeException(nameof(provider), provider, null);
            }
        }

        private async Task<IOAuthUserInfo?> GetOAuthUserInfoAsync(OAuthProvider provider, string accessToken)
        {
            string userInfoEndpoint = "";
            var client = _httpClientFactory.CreateClient();

            switch (provider)
            {
                case OAuthProvider.Google:
                    userInfoEndpoint = "https://www.googleapis.com/oauth2/v1/userinfo?alt=json";
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    break;
                case OAuthProvider.Yandex:
                    userInfoEndpoint = "https://login.yandex.ru/info?format=json";
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("OAuth", accessToken);
                    break;
            }

            var userInfoResponse = await client.GetAsync(userInfoEndpoint);
            if (!userInfoResponse.IsSuccessStatusCode)
                return null;

            var userInfoContent = await userInfoResponse.Content.ReadAsStringAsync();
            IOAuthUserInfo? userInfo = provider switch
            {
                OAuthProvider.Google => JsonSerializer.Deserialize<GoogleUserInfo>(userInfoContent),
                OAuthProvider.Yandex => JsonSerializer.Deserialize<YandexUserInfo>(userInfoContent),
                _ => null
            };
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

            var jwtTokens = await GetJwtTokensAsync(newUser);

            return ServiceResult<JwtTokens>.Success( jwtTokens );
        }

        private async Task<JwtTokens> GetJwtTokensAsync(User user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var token = _jwtService.GenerateAccessToken(user, roles);
            var refreshToken = _jwtService.GenerateRefreshToken(user);
            return new JwtTokens() { AccessToken = token, RefreshToken = refreshToken };
        }
    }
}
