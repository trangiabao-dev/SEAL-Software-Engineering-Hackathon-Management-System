using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SealHackathon.Domain.Exceptions
{
    public class ForbiddenException : AppException
    {
        /// <summary>
        /// Đã đăng nhập nhưng không có quyền - HTTP 403. Ví dụ: Leader cố gọi API chấm điểm của Judge
        /// </summary>
        public ForbiddenException(string message = "Bạn không có quyền thực hiện hành động này.")
            : base(message, 403)
        {
        }
    }
}