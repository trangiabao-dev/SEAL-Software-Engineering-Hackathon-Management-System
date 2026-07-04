using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SealHackathon.Domain.Interfaces.Repositories
{
    /// <summary>
    /// IUnitOfWork dùng để gom các thao tác database lại và lưu chung một lần.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        // Lấy ra Repository tương ứng của một Entity
        IGenericRepository<T> GetRepository<T>() where T : class;

        // Chốt và lưu toàn bộ thay đổi vào Database thành một Transaction
        Task<int> SaveChangesAsync();

        // Dọn sạch bộ nhớ theo dõi thay đổi (ChangeTracker) khi xảy ra lỗi trong xử lý lô (Batch Processing)
        void ClearChangeTracker();
    }
}
