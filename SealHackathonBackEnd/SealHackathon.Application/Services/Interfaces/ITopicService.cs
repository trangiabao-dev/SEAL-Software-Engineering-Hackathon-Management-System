using SealHackathon.Application.Common.Responses;
using SealHackathon.Application.DTOs.Topic;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SealHackathon.Application.Services.Interfaces
{
    // Giao diện (Interface) cho Topic Service
    public interface ITopicService
    {
        Task<ApiResponse<List<TopicResponse>>> GetTopicsByRoundIdAsync(int roundId);
        Task<ApiResponse<TopicResponse>> CreateTopicAsync(CreateTopicRequest request);
    }
}
