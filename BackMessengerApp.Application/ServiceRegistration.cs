using BackMessengerApp.Application.Interfaces;
using BackMessengerApp.Application.Services;
using BackMessengerApp.Core.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BackMessengerApp.Application
{
	public static class ServiceRegistration
	{
		public static void AddServiceApplication(this IServiceCollection services)
		{
			services.AddScoped<IJwtService, JwtService>();
			services.AddScoped<IUserService, UserService>();
		}

	}
}
