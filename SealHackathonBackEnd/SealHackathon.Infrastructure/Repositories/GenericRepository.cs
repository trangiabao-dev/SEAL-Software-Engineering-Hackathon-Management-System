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

        public async Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            // AsNoTracking() là tư duy tối ưu hiệu năng của Senior.
            // Nếu chỉ đọc dữ liệu lên để kiểm tra (như check Login) mà không sửa, 
            // ta nói EF Core không cần theo dõi (track) object này trên RAM làm gì cho nặng.
            return await _dbSet.AsNoTracking().FirstOrDefaultAsync(predicate);
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

        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }
    }
}