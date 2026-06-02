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
        // Lấy ra bản ghi đầu tiên thỏa mãn điều kiện, nếu không có trả về null
        Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Thực thi câu lệnh đếm (SELECT COUNT) trực tiếp dưới CSDL.
        /// BẮT BUỘC dùng hàm này khi chỉ cần biết số lượng, tuyệt đối không dùng GetAllAsync().Count để tránh tràn RAM.
        /// </summary>
        /// <param name="predicate">Điều kiện đếm (Ví dụ: x => x.IsDeleted == false)</param>
        Task<int> CountAsync(Expression<Func<T, bool>> predicate);

        // Thêm một bản ghi mới (Dùng cho chức năng Register sau này)
        Task AddAsync(T entity);

        //Expression<Func<T, bool>> predicate được hiểu là:
        //"Một câu lệnh điều kiện (trả về true/false) được
        //viết dưới dạng cấu trúc cây để EF Core có thể dịch thành câu lệnh WHERE trong SQL"
        //vd: SELECT TOP(1) * FROM[Account] WHERE[Username] = 'giabao'

        Task<List<T>> GetAllAsync(Expression<Func<T, bool>> predicate);
        void Update(T entity);

        // Bảo thêm 1
        void Delete(T entity);

    }
}
