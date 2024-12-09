using Microsoft.AspNetCore.Identity;

namespace BackMessengerApp.Core.Models
{
	public class User : IdentityUser
	{
		public string Name { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; }
	}
}
