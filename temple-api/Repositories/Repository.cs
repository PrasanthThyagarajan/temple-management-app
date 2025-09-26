using Microsoft.EntityFrameworkCore;
using TempleApi.Data;
using TempleApi.Repositories.Interfaces;
using System.Linq.Expressions;

namespace TempleApi.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly TempleDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(TempleDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<T?> GetByIdAsync(int id, params Expression<Func<T, object?>>[] includes)
        {
            var query = _dbSet.AsQueryable();
            
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            
            return await query.FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            // Check if the entity has IsActive property
            var hasIsActiveProperty = typeof(T).GetProperty("IsActive") != null;
            
            if (hasIsActiveProperty)
            {
                return await _dbSet
                    .Where(e => EF.Property<bool>(e, "IsActive"))
                    .OrderByDescending(e => EF.Property<int>(e, "Id"))
                    .ToListAsync();
            }
            
            return await _dbSet
                .OrderByDescending(e => EF.Property<int>(e, "Id"))
                .ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object?>>[] includes)
        {
            var query = _dbSet.AsQueryable();
            
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            
            // Check if the entity has IsActive property
            var hasIsActiveProperty = typeof(T).GetProperty("IsActive") != null;
            
            if (hasIsActiveProperty)
            {
                query = query.Where(e => EF.Property<bool>(e, "IsActive"));
            }
            
            return await query
                .OrderByDescending(e => EF.Property<int>(e, "Id"))
                .ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet
                .Where(predicate)
                .OrderByDescending(e => EF.Property<int>(e, "Id"))
                .ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object?>>[] includes)
        {
            var query = _dbSet.AsQueryable();
            
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            
            return await query
                .Where(predicate)
                .OrderByDescending(e => EF.Property<int>(e, "Id"))
                .ToListAsync();
        }

        public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object?>>[] includes)
        {
            var query = _dbSet.AsQueryable();
            
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            
            return await query.FirstOrDefaultAsync(predicate);
        }

        public virtual async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public virtual async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
            return entities;
        }

        public virtual async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task UpdateRangeAsync(IEnumerable<T> entities)
        {
            _dbSet.UpdateRange(entities);
            await _context.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task DeleteRangeAsync(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
            await _context.SaveChangesAsync();
        }

        public virtual async Task<bool> DeleteByIdAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null)
                return false;
                
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public virtual async Task<bool> SoftDeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null)
                return false;
                
            // Check if the entity has IsActive property
            var hasIsActiveProperty = typeof(T).GetProperty("IsActive") != null;
            
            if (hasIsActiveProperty)
            {
                var isActiveProperty = typeof(T).GetProperty("IsActive");
                if (isActiveProperty != null)
                {
                    isActiveProperty.SetValue(entity, false);
                    await UpdateAsync(entity);
                    return true;
                }
            }
            
            return false;
        }

        public virtual async Task<int> CountAsync()
        {
            return await _dbSet.CountAsync();
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.CountAsync(predicate);
        }

        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        public virtual async Task<bool> ExistsByIdAsync(int id)
        {
            return await _dbSet.AnyAsync(e => EF.Property<int>(e, "Id") == id);
        }

        public virtual async Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize)
        {
            return await _dbSet
                .OrderByDescending(e => EF.Property<int>(e, "Id"))
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<T, bool>> predicate)
        {
            return await _dbSet
                .Where(predicate)
                .OrderByDescending(e => EF.Property<int>(e, "Id"))
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
