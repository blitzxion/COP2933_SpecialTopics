using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace LogFileParser.Helpers
{
	public static class EntityPerformance
	{

		public static void SaveRange<TContext, TEntity>(
			TContext context, 
			IEnumerable<TEntity> entities, 
			Action<int> onSave = null,
			bool disposeContext = true)

			where TContext : DbContext
			where TEntity : class
		{
			try
			{
				int position = 0;
				context.Configuration.AutoDetectChangesEnabled = false; // Required, major performance improvement
				foreach (TEntity entity in entities)
				{
					context = SaveToContext(context, entity, ++position, onSave: onSave);
				}
			}
			finally
			{
				if(disposeContext && context != null)
				{
					context.SaveChanges();
					context.Dispose();
				}
			}
		}

		public static void DeleteRange<TContext, TEntity>(TContext context,
			IEnumerable<TEntity> entities,
			Action<int> onSave = null,
			bool disposeContext = true)
			where TContext : DbContext
			where TEntity : class
		{
			try
			{
				int position = 0;
				context.Configuration.AutoDetectChangesEnabled = false; // Required, major performance improvement
				foreach (TEntity entity in entities)
				{
					context = RemoveFromContext(context, entity, ++position, onSave: onSave);
				}
			}
			finally
			{
				if (disposeContext && context != null)
				{
					context.SaveChanges();
					context.Dispose();
				}
			}
		}

		/// <summary>
		/// Prepare or Save a record to the database. This is a high performance method helper and should only be used in a loop.
		/// </summary>
		/// <typeparam name="TContext">The working context type</typeparam>
		/// <typeparam name="TEntity">The DBSet model type</typeparam>
		/// <param name="context">The working context. Will return existing or new context.</param>
		/// <param name="entity">The object that will has the changes</param>
		/// <param name="count">Position in your loop</param>
		/// <param name="commitCount">How many iterations before your context is saved and made anew?</param>
		/// <param name="recreateContext">Once the commitCount is reached, recreate the context and return it?</param>
		/// <returns></returns>
		public static TContext SaveToContext<TContext, TEntity>(TContext context, TEntity entity, int count, int commitCount = 1000, bool recreateContext = true, Action<int> onSave = null)
			where TContext : DbContext
			where TEntity : class
		{
			context.Set<TEntity>().Add(entity);
			return SaveOrCreateContext(context, count, commitCount, recreateContext, onSave);
		}

		/// <summary>
		/// Prepare or Save the removal to the database. This is a high performance method helper and should only be used in a loop.
		/// </summary>
		/// <typeparam name="TContext">The working context type</typeparam>
		/// <typeparam name="TEntity">The DBSet model type</typeparam>
		/// <param name="context">The working context. Will return existing or new context.</param>
		/// <param name="entity">The DbSet object that will has the changes</param>
		/// <param name="count">Position in your loop</param>
		/// <param name="commitCount">How many iterations before your context is saved and made anew?</param>
		/// <param name="recreateContext">Once the commitCount is reached, recreate the context and return it?</param>
		/// <returns></returns>
		public static TContext RemoveFromContext<TContext, TEntity>(TContext context, TEntity entity, int count, int commitCount = 1000, bool recreateContext = true, Action<int> onSave = null)
			where TContext : DbContext
			where TEntity : class
		{
			context.Set<TEntity>().Remove(entity);
			return SaveOrCreateContext(context, count, commitCount, recreateContext, onSave);
		}

		static TContext SaveOrCreateContext<TContext>(TContext context, int count, int commitCount = 1000, bool recreateContext = true, Action<int> onSave = null)
			where TContext : DbContext
		{
			if (count % commitCount == 0)
			{
				context.SaveChanges();

				onSave?.Invoke(count);

				if (recreateContext)
				{
					context.Dispose();
					context = (TContext)Activator.CreateInstance(typeof(TContext));
					context.Configuration.AutoDetectChangesEnabled = false;
					
				}
			}

			return context;
		}
	}
}