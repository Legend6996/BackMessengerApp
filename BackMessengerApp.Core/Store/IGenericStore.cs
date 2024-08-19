using BackMessengerApp.Core.Common.BaseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BackMessengerApp.Core.Store
{
	public interface IGenericStore<TModel> where TModel : IModel
	{
		Task<TModel?> GetByIdAsync(int Id, params Expression<Func<TModel, object>>[] includes);
		Task<IEnumerable<TModel>> GetAllAsync(params Expression<Func<TModel, object>>[] includes);
		IQueryable<TModel> Where(Expression<Func<TModel, bool>> predicate, params Expression<Func<TModel, object>>[] includes);
		Task<TModel> AddAsync(TModel model);
		Task RemoveAsync(int Id);
		Task<TModel> UpdateAsync(TModel model);
	}
}
