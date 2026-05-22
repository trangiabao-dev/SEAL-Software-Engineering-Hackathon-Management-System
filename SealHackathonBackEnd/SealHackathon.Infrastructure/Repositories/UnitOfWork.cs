using SealHackathon.Domain.Interfaces.Repositories;
using SealHackathon.Infrastructure.Data;

namespace SealHackathon.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly SealDbContext _context;
        private readonly Dictionary<string, object> _repositories = new();

        public UnitOfWork(SealDbContext context)
        {
            _context = context;
        }

        public IGenericRepository<T> GetRepository<T>() where T : class
        {
            var type = typeof(T).Name;

            if (!_repositories.ContainsKey(type))
            {
                _repositories[type] = new GenericRepository<T>(_context);
            }

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