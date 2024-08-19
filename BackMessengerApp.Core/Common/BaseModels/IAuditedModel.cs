using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackMessengerApp.Core.Common.BaseModels
{
    public interface IAuditedModel : IAuditedModel<int>
	{

	}
	public interface IAuditedModel<TKey> : IModel<TKey>
	{
		DateTime CreatedAt { get; set; }
		DateTime EditedAt { get; set; }
	}
}
