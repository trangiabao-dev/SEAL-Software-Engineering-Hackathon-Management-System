using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SealHackathon.Domain.Exceptions
{
    // Class cha — tất cả custom exception đều kế thừa từ đây
    // Lý do: ExceptionMiddleware chỉ cần catch AppException là bắt được tất cả
    public class AppException : Exception
    {
        public int StatusCode { get; }

        public AppException(string message, int statusCode = 500)
            : base(message)
        {
            StatusCode = statusCode;
        }
        // aaa
    }
}
