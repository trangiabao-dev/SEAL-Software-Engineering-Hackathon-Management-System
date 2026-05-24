using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SealHackathon.Application.DTOs.Auth
{
    public class RejectRequest
    {
        public string Reason { get; set; } = null!;
    }
}
