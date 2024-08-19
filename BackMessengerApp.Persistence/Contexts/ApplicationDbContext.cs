using BackMessengerApp.Core.Common.BaseModels;
using BackMessengerApp.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace BackMessengerApp.Persistence.Contexts
{
	public class ApplicationDbContext : IdentityDbContext<User, Role, string>
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }



		public override int SaveChanges(bool acceptAllChangesOnSuccess)
		{
			AuditedSave();
			return base.SaveChanges(acceptAllChangesOnSuccess);
		}

		public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
		{
			AuditedSave();
			return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
		}

		private void AuditedSave()
		{
			foreach (var item in ChangeTracker.Entries())
			{
				if (item.Entity is AuditedModel modelRef)
				{
					switch (item.State)
					{
						case EntityState.Added:
							{
								modelRef.CreatedAt = DateTime.UtcNow;
								break;
							}
						case EntityState.Modified:
							{
								Entry(modelRef).Property(m => m.CreatedAt).IsModified = false;
								modelRef.EditedAt = DateTime.UtcNow;
								break;
							}
					}
				}
			}
		}
	}
}
