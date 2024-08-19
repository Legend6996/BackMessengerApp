using BackMessengerApp.Core.Common.BaseModels;
using BackMessengerApp.Core.Store;
using BackMessengerApp.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BackMessengerApp.Persistence.Repositories
{
	public class GenericRepository<TModel> : IGenericStore<TModel> where TModel : class, IModel
	{
		private readonly ApplicationDbContext _dbContext;
		private readonly DbSet<TModel> _dbSet;

		public GenericRepository(ApplicationDbContext dbContext)
		{
			_dbContext = dbContext;
			_dbSet = dbContext.Set<TModel>();	
			
		}

		public async Task<TModel?> GetByIdAsync(int Id, params Expression<Func<TModel, object>>[] includes)
		{
			var query = GetQueryWithIncludes(includes);

			return await query.FirstOrDefaultAsync(m => m.Id == Id);
		}

		public async Task<IEnumerable<TModel>> GetAllAsync(params Expression<Func<TModel, object>>[] includes)
		{
			var query = GetQueryWithIncludes(includes);

			return await query.ToListAsync();
		}

		public IQueryable<TModel> Where(Expression<Func<TModel, bool>> predicate, params Expression<Func<TModel, object>>[] includes)
		{
			var query = GetQueryWithIncludes(includes);

			return query.Where(predicate);
		}

		public async Task<TModel> AddAsync(TModel model)
		{
			await _dbSet.AddAsync(model);
			await _dbContext.SaveChangesAsync();
			return model;
		}

		public async Task RemoveAsync(int Id)
		{
			var model = await _dbSet.FindAsync(Id);
			if (model == null)
			{
				throw new KeyNotFoundException("Model not found");
			}

			_dbSet.Remove(model);
			await _dbContext.SaveChangesAsync();
		}

		public async Task<TModel> UpdateAsync(TModel model)
		{
			var existModel = await _dbSet.FindAsync(model.Id);
			if (existModel == null)
			{
				throw new KeyNotFoundException("Model not found");
			}

			_dbSet.Update(model);
			await _dbContext.SaveChangesAsync();
			return model;
		}

		private IQueryable<TModel> GetQueryWithIncludes(params Expression<Func<TModel, object>>[] includes)
		{
			var query = _dbSet.AsQueryable<TModel>();
			foreach (var include in includes)
			{
				query = query.Include(include);
			}
			return query;
		}
	}
}
