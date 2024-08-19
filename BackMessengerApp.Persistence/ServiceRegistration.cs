using BackMessengerApp.Core.Settings;
using BackMessengerApp.Core.Store;
using BackMessengerApp.Persistence.Contexts;
using BackMessengerApp.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BackMessengerApp.Persistence
{
	public static class ServiceRegistration
	{
		public static void AddServicePersistence(this IServiceCollection services)
		{
			services.AddScoped(typeof(IGenericStore<>), typeof(GenericRepository<>));

			var settings = services.BuildServiceProvider().GetRequiredService<IOptions<AppSettings>>();

			string? dbConnection = Environment.GetEnvironmentVariable("DATABASE_CONNECTION");

			if(dbConnection == null)
			{
				dbConnection = settings.Value.DatabaseSettings.ConnectionString;
			}

			services.AddDbContext<ApplicationDbContext>(options => 
				options.UseNpgsql(dbConnection));
		}
	}
}
