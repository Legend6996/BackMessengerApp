using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackMessengerApp.Core.Settings
{
	public class JwtSettings
	{
		public string AccessKey { get; set; }
		public string RefreshKey { get; set; }
		public string Issuer { get; set; }
		public string Audience { get; set; }
		public string AccessExpireMinutes { get; set; }
		public string RefreshExpireMinutes { get; set; }
	}
}
