using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SealHackathon.Domain.Exceptions
{
    public class ConflictException : AppException
    {
        /// <summary>
        /// Dữ liệu bị trùng hoặc xung đột trạng thái — HTTP 409. Ví dụ: Đăng ký email đã tồn tại trong hệ thống
        /// </summary>
        public ConflictException(string message)
            : base(message, 409)
        {
        }
    }
}