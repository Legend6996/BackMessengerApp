using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackMessengerApp.Core.Common.BaseModels
{
	public class AuditedModel : Model, IAuditedModel
	{
		public DateTime CreatedAt { get; set; }
		public DateTime EditedAt { get; set; }
	}
	public class AuditedModel<Tkey> : Model<Tkey>, IAuditedModel<Tkey>
	{
		public DateTime CreatedAt { get; set; }
		public DateTime EditedAt { get; set; }
	}
}
