using Microsoft.EntityFrameworkCore;
using TempleApi.Data;
using TempleApi.Repositories.Interfaces;
using System.Linq.Expressions;

namespace TempleApi.Repositories
{
    public class FactoryRepository<T> : IRepository<T> where T : class
    {
        protected readonly IDbContextFactory _contextFactory;

        public FactoryRepository(IDbContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            using var context = _contextFactory.CreateTempleDbContext();
            return await context.Set<T>().FindAsync(id);
        }

        public virtual async Task<T?> GetByIdAsync(int id, params Expression<Func<T, object>>[] includes)
        {
            using var context = _contextFactory.CreateTempleDbContext();
            var query = context.Set<T>().AsQueryable();
            
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            
            return await query.FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            using var context = _contextFactory.CreateTempleDbContext();
            return await context.Set<T>().ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includes)
        {
            using var context = _contextFactory.CreateTempleDbContext();
            var query = context.Set<T>().AsQueryable();
            
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            
            return await query.ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            using var context = _contextFactory.CreateTempleDbContext();
            return await context.Set<T>().Where(predicate).ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes)
        {
            using var context = _contextFactory.CreateTempleDbContext();
            var query = context.Set<T>().AsQueryable();
            
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            
            return await query.Where(predicate).ToListAsync();
        }

        public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            using var context = _contextFactory.CreateTempleDbContext();
            return await context.Set<T>().FirstOrDefaultAsync(predicate);
        }

        public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes)
        {
            using var context = _contextFactory.CreateTempleDbContext();
            var query = context.Set<T>().AsQueryable();
            
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            
            return await query.FirstOrDefaultAsync(predicate);
        }

        public virtual async Task<T> AddAsync(T entity)
        {
            using var context = _contextFactory.CreateTempleDbContext();
            var entry = await context.Set<T>().AddAsync(entity);
            await context.SaveChangesAsync();
            return entry.Entity;
        }

        public virtual async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities)
        {
            using var context = _contextFactory.CreateTempleDbContext();
            await context.Set<T>().AddRangeAsync(entities);
            await context.SaveChangesAsync();
            return entities;
        }

        public virtual async Task UpdateAsync(T entity)
        {
            using var context = _contextFactory.CreateTempleDbContext();
            context.Set<T>().Update(entity);
            await context.SaveChangesAsync();
        }

        public virtual async Task UpdateRangeAsync(IEnumerable<T> entities)
        {
            using var context = _contextFactory.CreateTempleDbContext();
            context.Set<T>().UpdateRange(entities);
            await context.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(T entity)
        {
            using var context = _contextFactory.CreateTempleDbContext();
            context.Set<T>().Remove(entity);
            await context.SaveChangesAsync();
        }

        public virtual async Task DeleteRangeAsync(IEnumerable<T> entities)
        {
            using var context = _contextFactory.CreateTempleDbContext();
            context.Set<T>().RemoveRange(entities);
            await context.SaveChangesAsync();
        }

        public virtual async Task<bool> DeleteByIdAsync(int id)
        {
            using var context = _contextFactory.CreateTempleDbContext();
            var entity = await context.Set<T>().FindAsync(id);
            if (entity == null)
                return false;

            context.Set<T>().Remove(entity);
            await context.SaveChangesAsync();
            return true;
        }

        public virtual async Task<bool> SoftDeleteAsync(int id)
        {
            using var context = _contextFactory.CreateTempleDbContext();
            var entity = await context.Set<T>().FindAsync(id);
            if (entity == null)
                return false;

            // Check if entity has IsActive property
            var isActiveProperty = typeof(T).GetProperty("IsActive");
            if (isActiveProperty != null && isActiveProperty.CanWrite)
            {
                isActiveProperty.SetValue(entity, false);
                context.Set<T>().Update(entity);
                await context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public virtual async Task<int> CountAsync()
        {
            using var context = _contextFactory.CreateTempleDbContext();
            return await context.Set<T>().CountAsync();
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            using var context = _contextFactory.CreateTempleDbContext();
            return await context.Set<T>().CountAsync(predicate);
        }

        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            using var context = _contextFactory.CreateTempleDbContext();
            return await context.Set<T>().AnyAsync(predicate);
        }

        public virtual async Task<bool> ExistsByIdAsync(int id)
        {
            using var context = _contextFactory.CreateTempleDbContext();
            return await context.Set<T>().AnyAsync(e => EF.Property<int>(e, "Id") == id);
        }

        public virtual async Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize)
        {
            using var context = _contextFactory.CreateTempleDbContext();
            return await context.Set<T>()
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<T, bool>> predicate)
        {
            using var context = _contextFactory.CreateTempleDbContext();
            return await context.Set<T>()
                .Where(predicate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
