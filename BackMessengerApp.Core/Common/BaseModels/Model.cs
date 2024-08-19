using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackMessengerApp.Core.Common.BaseModels
{
	public class Model : IModel
	{
		public int Id { get; set; }
	}
    public class Model<Tkey> : IModel<Tkey>
    {
        public Tkey Id { get; set; }
    }
}

