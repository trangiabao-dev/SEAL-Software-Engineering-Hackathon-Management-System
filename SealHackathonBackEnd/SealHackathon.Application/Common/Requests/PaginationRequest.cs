using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SealHackathon.Application.Common.Requests
{
    public class PaginationRequest
    {
        // Trang hiện tại — mặc định là trang 1
        public int PageNumber { get; set; } = 1;

        // Số bản ghi mỗi trang — mặc định là 10
        public int PageSize { get; set; } = 10;

        // Giới hạn tối đa PageSize — tránh client gửi PageSize=99999
        // để lấy hết data một lúc, phá vỡ mục đích pagination
        public const int MaxPageSize = 50;

        // Số bản ghi cần bỏ qua — dùng trong câu query LINQ/SQL
        // Ví dụ: PageNumber=2, PageSize=10 → Skip=10 → bỏ qua 10 bản ghi đầu
        public int Skip => (PageNumber - 1) * PageSize;
    }
}