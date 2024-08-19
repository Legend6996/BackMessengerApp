using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackMessengerApp.Core.Common.BaseModels
{
	public interface IModel : IModel<int>
	{

	}

	public interface IModel<TKey>
	{
		TKey Id { get; set; }
	}
}
