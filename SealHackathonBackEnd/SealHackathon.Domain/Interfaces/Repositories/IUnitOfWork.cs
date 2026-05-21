using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SealHackathon.Domain.Interfaces.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        // Lấy ra Repository tương ứng của một Entity
        IGenericRepository<T> GetRepository<T>() where T : class;

        // Chốt và lưu toàn bộ thay đổi vào Database thành một Transaction
        Task<int> SaveChangesAsync();
    }
}
