using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace SealHackathon.Application.Common.Responses
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string>? Errors { get; set; }

        // Dùng khi API thành công — có data trả về
        public static ApiResponse<T> SuccessResult(T data, string message = "Thành công.")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                Errors = null
            };
        }

        // Dùng khi API thất bại — không có data, chỉ có message lỗi
        public static ApiResponse<T> FailResult(string message, List<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Data = default,
                Errors = errors
                //Data = null — compiler sẽ cảnh báo vì int không thể gán null.
                //default thông minh hơn: tự động chọn giá trị mặc định phù hợp với kiểu T:

                //T là string → default = null
                //T là int → default = 0
                //T là class → default = null
            };
        }
    }
}