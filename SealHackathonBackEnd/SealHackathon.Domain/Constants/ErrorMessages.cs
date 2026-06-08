namespace SealHackathon.Domain.Constants
{
    public static class ErrorMessages
    {
        public static class Team
        {
            public const string NotFound = "Không tìm thấy đội thi.";
            public const string NoUpdatePermission = "Bạn không có quyền chỉnh sửa đội này.";
            public const string NoAddMemberPermission = "Bạn không có quyền thêm thành viên vào đội này.";
            public const string NoUpdateMemberPermission = "Bạn không có quyền chỉnh sửa thành viên của đội này.";
            public const string NoDeleteMemberPermission = "Bạn không có quyền xóa thành viên của đội này.";
            public const string AlreadyHasTeamInEvent = "Bạn đã có đội trong Event này.";
            public const string TrackFull = "Track này đã đạt số lượng đội tối đa.";
            public const string ApprovedOnlyGithubCanChange = "Đội thi đã được duyệt. Bạn chỉ được phép cập nhật Link Github.";
            public const string NameAlreadyUsed = "Tên đội này đã có người đăng ký. Vui lòng chọn tên khác.";
            public const string OnlyPendingCanApprove = "Chỉ có thể duyệt team đang ở trạng thái Pending.";
            public const string AlreadyDisqualified = "Đội thi này đã bị loại trước đó.";
            public const string MentorNotInEvent = "Tài khoản được chọn chưa được phân quyền Mentor trong Event này.";
            public const string MentorNotAssignedToTrack = "Mentor này chưa được phân công vào Track của đội thi.";
            public const string MentorMaxTeamsReached = "Mentor này đã hướng dẫn tối đa số đội cho phép.";
        }

        public static class TeamMember
        {
            public const string NotFound = "Không tìm thấy thành viên đội.";
            public const string CannotDeleteLeader = "Không thể xóa Đội trưởng. Bạn không đủ quyền hạn.";
            public const string MaxMembersReached = "Đội đã đạt giới hạn số lượng thành viên.";
            public const string MinMembersRequired = "Đội cần đủ số lượng thành viên tối thiểu.";
            public const string ApprovedTeamMinMembersRequired = "Đội đã được duyệt. Cần giữ đủ số lượng thành viên tối thiểu.";
            public const string StudentCodeAlreadyUsedInEvent = "Mã sinh viên đã tồn tại trong một đội khác cùng Event.";
        }

        public static class Submission
        {
            public const string NotFound = "Không tìm thấy bài nộp.";
            public const string TeamNotFound = "Không tìm thấy đội thi của bài nộp này.";
            public const string NoUpdatePermission = "Bạn không có quyền cập nhật bài nộp này.";
            public const string NoViewPermission = "Bạn không có quyền xem bài nộp này.";
            public const string NoTeamInRoundTrack = "Bạn không có team thuộc Track của Round này.";
            public const string TeamNotApproved = "Chỉ team đã được duyệt mới được nộp bài.";
            public const string AlreadySubmitted = "Team này đã nộp bài cho Round này rồi.";
            public const string DeadlinePassed = "Đã hết thời gian nộp bài của Round này.";
            public const string UpdateDeadlinePassed = "Đã quá hạn cập nhật bài nộp của Round này.";
            public const string CannotUpdateDisqualified = "Bài nộp đã bị loại, không thể cập nhật.";
            public const string AlreadyDisqualified = "Bài nộp này đã bị loại trước đó.";
            public const string NeedAtLeastOneLink = "Cần cung cấp ít nhất một link bài nộp.";
            public const string JudgeNotAssignedToRound = "Bạn không được phân công chấm Round này.";
            public const string TeamGithubRepoRequired = "Vui lòng cập nhật link GitHub của đội trước khi nộp bài.";
        }

        public static class Common
        {
            public const string InvalidStatus = "Status không hợp lệ.";
            public const string InvalidPageNumber = "PageNumber phải lớn hơn hoặc bằng 1.";
            public const string InvalidPageSize = "PageSize không hợp lệ.";
            public const string InvalidAccount = "Tài khoản của bạn không tồn tại hoặc đã bị vô hiệu hóa.";
            public const string InvalidRoundId = "RoundId không hợp lệ.";
            public const string InvalidSubmissionId = "SubmissionId không hợp lệ.";
            public const string InvalidMentorId = "MentorId không hợp lệ.";
            public const string RoundNotFound = "Không tìm thấy vòng thi.";
            public const string TrackNotFound = "Không tìm thấy hạng mục.";
            public const string MentorNotFound = "Không tìm thấy Mentor.";
        }
    }
}