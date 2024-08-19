using BackMessengerApp.Core.Models;
using BackMessengerApp.Core.Settings;
using BackMessengerApp.Persistence.Contexts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace BackMessengerApp.API
{
	public static class Startup
	{
		public static void AddServicesGeneral(this IServiceCollection services, WebApplicationBuilder builder)
		{

			if (builder.Environment.IsProduction())
			{
				Console.WriteLine("__________________________PRODUCTION__________________________");
			}
			else if (builder.Environment.IsDevelopment())
			{
				Console.WriteLine("__________________________DEVELOPMENT__________________________");
			}

			AddAuth(builder);

			services.AddIdentityCore<User>()
				.AddRoles<Role>()
				.AddEntityFrameworkStores<ApplicationDbContext>()
				.AddDefaultTokenProviders();

			services.AddControllers();
			services.AddEndpointsApiExplorer();
			services.AddSwaggerGen();
		}

		public static void AddConfigurationGeneral(this WebApplication app)
		{
			var settings = app.Services.GetRequiredService<IOptions<AppSettings>>().Value;

			app.UseCors(policy => policy.WithOrigins(settings.FrontURL)
				.AllowAnyHeader()
				.AllowAnyMethod()
				.AllowCredentials());

			ApplyDatabaseMigrations(app);

			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}

			app.UseHttpsRedirection();
			
			app.UseAuthentication();
			app.UseAuthorization();

			app.MapControllers();

		}
		private static void ApplyDatabaseMigrations(WebApplication app)
		{
			using (var scope = app.Services.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
				dbContext.Database.Migrate();
			}
		}

		private static void AddAuth(WebApplicationBuilder builder)
		{
			var settings = builder.Services.BuildServiceProvider().GetRequiredService<IOptions<JwtSettings>>();

			builder.Services.AddAuthentication(options =>
			{
				options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			})
			.AddJwtBearer(options =>
			{
				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true,
					ValidIssuer = settings.Value.Issuer,
					ValidAudience = settings.Value.Audience,
					IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Value.AccessKey))
				};
			});

			builder.Services.AddAuthorization();
		}
	}
}
