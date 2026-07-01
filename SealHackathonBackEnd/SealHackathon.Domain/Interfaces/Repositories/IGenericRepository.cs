using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SealHackathon.Domain.Interfaces.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        /// <summary>
        /// GetFirstOrDefaultAsync dùng khi chỉ đọc. Vì có AsNoTracking nên EF Core không theo dõi entity đó. 
        /// Nếu sửa object lấy từ hàm này rồi gọi SaveChangesAsync thì thường không lưu thay đổi.
        /// Nó không tự ghi đè database, trừ khi mình gọi Update(entity) hoặc attach lại entity vào DbContext.
        Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

        IQueryable<T> GetQueryable(Expression<Func<T, bool>>? predicate = null);

        /// <summary>
        /// Thực thi câu lệnh đếm (SELECT COUNT) trực tiếp dưới CSDL.
        /// BẮT BUỘC dùng hàm này khi chỉ cần biết số lượng, tuyệt đối không dùng GetAllAsync().Count để tránh tràn RAM.
        /// </summary>
        /// <param name="predicate">Điều kiện đếm (Ví dụ: x => x.IsDeleted == false)</param>
        Task<int> CountAsync(Expression<Func<T, bool>> predicate);
        Task<Dictionary<TKey, int>> CountByGroupAsync<TKey>(
            Expression<Func<T, bool>> predicate,
            Expression<Func<T, TKey>> groupBy)
            where TKey : notnull;

        // Thêm một bản ghi mới (Dùng cho chức năng Register sau này)
        Task AddAsync(T entity);

        //Expression<Func<T, bool>> predicate được hiểu là:
        //"Một câu lệnh điều kiện (trả về true/false) được
        //viết dưới dạng cấu trúc cây để EF Core có thể dịch thành câu lệnh WHERE trong SQL"
        //vd: SELECT TOP(1) * FROM[Account] WHERE[Username] = 'giabao'

        Task<List<T>> GetAllAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Cần hàm này khi service phải đọc entity chính kèm dữ liệu liên quan.
        /// Nếu không có Include, service dễ rơi vào lỗi N+1 query: lấy danh sách trước rồi query từng dòng liên quan sau.
        /// Bên trong hàm sẽ nhận điều kiện lọc và các navigation property cần lấy kèm, rồi để EF Core sinh truy vấn phù hợp.
        /// </summary>
        Task<List<T>> GetAllWithIncludeAsync(
            Expression<Func<T, bool>> predicate,
            params Expression<Func<T, object>>[] includes);

        void Update(T entity);

        void Delete(T entity);

        /// <summary>
        /// GetFirstOrDefaultTrackingAsync dùng khi lấy entity ra để sửa. 
        /// EF Core theo dõi entity đó, biết property nào thay đổi và SaveChangesAsync sẽ update xuống database.
        /// </summary>
        Task<T?> GetFirstOrDefaultTrackingAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Lấy dữ liệu phân trang có sắp xếp cố định.
        /// Nếu phân trang mà không có OrderBy, dữ liệu giữa các trang có thể bị nhảy khi database lớn hoặc có dữ liệu mới.
        /// </summary>
        Task<List<T>> GetPagedAsync<TKey>(
            Expression<Func<T, bool>> predicate,
            Expression<Func<T, TKey>> orderBy,
            int skip,
            int take,
            bool descending = false);


    }
}
