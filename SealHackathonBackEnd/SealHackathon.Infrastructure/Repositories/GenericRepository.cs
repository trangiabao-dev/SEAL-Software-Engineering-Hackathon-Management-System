using Microsoft.EntityFrameworkCore;
using SealHackathon.Domain.Interfaces.Repositories;
using SealHackathon.Infrastructure.Data;
using System.Linq.Expressions;

namespace SealHackathon.Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly SealDbContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(SealDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public async Task<List<T>> GetAllAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AsNoTracking().Where(predicate).ToListAsync();
        }
        // Thức 

        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        // Bảo thêm 1
        /// <summary>
        /// Gọi hàm CountAsync của Entity Framework Core để đếm dữ liệu bằng SQL Server.
        /// </summary>
        public async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.CountAsync(predicate);
        }

        public async Task<Dictionary<TKey, int>> CountByGroupAsync<TKey>(
            Expression<Func<T, bool>> predicate,
            Expression<Func<T, TKey>> groupBy)
            where TKey : notnull
        {
            return await _dbSet.AsNoTracking()
                .Where(predicate)
                .GroupBy(groupBy)
                .Select(g => new
                {
                    Key = g.Key,
                    Count = g.Count()
                })
                .ToDictionaryAsync(x => x.Key, x => x.Count);
        }

        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

        /// <summary>
        /// Chỉ đọc data, KHÔNG sửa (Kiểm tra tên team trùng, check track tồn tại)
        /// </summary>
        public async Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AsNoTracking().FirstOrDefaultAsync(predicate);
        }

        /// <summary>
        /// Bảo thêm 2: (Đọc xong rồi SỬA entity đó: Lấy team ra → đổi status → save)
        /// </summary>
        public async Task<T?> GetFirstOrDefaultTrackingAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate); // KHÔNG có AsNoTracking
        }

        // Bảo thêm 3
        public async Task<List<T>> GetPagedAsync(Expression<Func<T, bool>> predicate, int skip, int take)
        {
            return await _dbSet.AsNoTracking()
                .Where(predicate)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }
    }
}