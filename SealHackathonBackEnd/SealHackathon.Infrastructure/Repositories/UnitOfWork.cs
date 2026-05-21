using SealHackathon.Domain.Interfaces.Repositories;
using SealHackathon.Infrastructure.Data;
using System.Collections;

namespace SealHackathon.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly SealDbContext _context;
        private Hashtable _repositories;

        public UnitOfWork(SealDbContext context)
        {
            _context = context;
        }

        public IGenericRepository<T> GetRepository<T>() where T : class
        {
            if (_repositories == null)
                _repositories = new Hashtable();

            var type = typeof(T).Name;

            // Nếu trong Hashtable chưa có Repository của bảng này thì mới tạo mới
            if (!_repositories.ContainsKey(type))
            {
                var repositoryType = typeof(GenericRepository<>);
                var repositoryInstance = Activator.CreateInstance(repositoryType.MakeGenericType(typeof(T)), _context);
                _repositories.Add(type, repositoryInstance);
            }

            // Nếu có rồi thì lấy ra dùng luôn
            return (IGenericRepository<T>)_repositories[type]!;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}