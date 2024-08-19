using Microsoft.AspNetCore.Identity;

namespace BackMessengerApp.Core.Models
{
	public class User : IdentityUser
	{
		public DateTime CreatedAt { get; set; }
	}
}
