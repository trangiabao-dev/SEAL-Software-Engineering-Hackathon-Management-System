using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SealHackathon.Domain.Exceptions
{
    // Dùng khi dữ liệu đầu vào không hợp lệ — HTTP 400
    // Ví dụ: Login sai password, nhập thiếu field bắt buộc
    public class BadRequestException : AppException
    {
        public BadRequestException(string message)
            : base(message, 400)
        {
        }
    }
}
