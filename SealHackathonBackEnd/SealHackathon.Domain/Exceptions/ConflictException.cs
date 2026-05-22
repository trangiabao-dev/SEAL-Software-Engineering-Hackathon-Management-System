using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SealHackathon.Domain.Exceptions
{
    // Dùng khi dữ liệu bị trùng lặp — HTTP 409
    // Ví dụ: Đăng ký email đã tồn tại trong hệ thống
    public class ConflictException : AppException
    {
        public ConflictException(string message)
            : base(message, 409)
        {
        }
    }
}