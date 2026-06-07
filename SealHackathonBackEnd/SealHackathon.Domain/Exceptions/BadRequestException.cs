using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SealHackathon.Domain.Exceptions
{
    public class BadRequestException : AppException
    {
        /// <summary>
        /// Request sai dữ liệu, sai rule nhập vào — HTTP 400. Ví dụ: Login sai password, nhập thiếu field bắt buộc
        /// </summary>
        public BadRequestException(string message)
            : base(message, 400)
        {
        }
    }
}
