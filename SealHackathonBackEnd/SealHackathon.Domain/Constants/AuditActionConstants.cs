namespace SealHackathon.Domain.Constants;

/// <summary>
/// Tên các hành động được lưu vào AuditLog.
/// Hiện tại AuditLog chỉ tập trung vào lịch sử chấm điểm để tăng tính minh bạch học thuật.
/// </summary>
public static class AuditActionConstants
{
    public static class ScoreAudit
    {
        public const string Create = "Score.Create";
        public const string Update = "Score.Update";
    }

    public static class RankingAudit
    {
        public const string Calculate = "Ranking.Calculate";
    }
}