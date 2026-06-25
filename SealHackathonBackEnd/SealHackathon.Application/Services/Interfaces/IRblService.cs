using System.Collections.Generic;
using System.Threading.Tasks;
using SealHackathon.Application.DTOs.Rbl;

namespace SealHackathon.Application.Services.Interfaces
{
    public interface IRblService
    {
        Task<byte[]> ExportAnonymousScoresCsvAsync(int eventId);
        Task<List<CriterionVarianceResponse>> GetCriteriaVarianceAsync(int eventId);
    }
}
