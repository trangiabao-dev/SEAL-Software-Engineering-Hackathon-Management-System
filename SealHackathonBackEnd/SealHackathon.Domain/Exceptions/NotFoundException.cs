using SealHackathon.Domain.Entities;
using SealHackathon.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SealHackathon.Domain.Exceptions
{
    public class NotFoundException : AppException
    {
        // Dùng khi không tìm thấy resource — HTTP 404
        // Ví dụ: Tìm Account theo Id nhưng không có trong DB
        public NotFoundException(string resourceName, object key)
            : base($"{resourceName} with id '{key}' not exist.", 404)
        {
        }
    }
}
//Cách dùng sau này:
//csharpthrow new NotFoundException("Account", accountId);