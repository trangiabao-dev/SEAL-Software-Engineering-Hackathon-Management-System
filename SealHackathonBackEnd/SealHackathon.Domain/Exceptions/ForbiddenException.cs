using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SealHackathon.Domain.Exceptions
{
    // Dùng khi user đã login nhưng không có quyền — HTTP 403
    // Ví dụ: Leader cố gọi API chấm điểm của Judge
    public class ForbiddenException : AppException
    {
        public ForbiddenException(string message = "Bạn không có quyền thực hiện hành động này.")
            : base(message, 403)
        {
        }
    }
}