using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SealHackathon.Application.DTOs.Auth
{
    public class AccountPendingResponse
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string SystemRole { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
