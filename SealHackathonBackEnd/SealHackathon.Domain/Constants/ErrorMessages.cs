namespace SealHackathon.Domain.Constants
{
    /// <summary>
    /// Message lỗi dùng chung
    /// </summary>
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
            public const string TeamIdsRequired = "Cần chọn ít nhất một đội để phân công Mentor.";
            public const string TeamNotInTrack = "Một hoặc nhiều đội được chọn không thuộc Track này.";
            public const string OnlyApprovedCanAssignMentor = "Chỉ có thể phân Mentor cho đội đã được duyệt.";
        }

        public static class TeamMember
        {
            public const string NotFound = "Không tìm thấy thành viên đội.";
            public const string CannotDeleteLeader = "Không thể xóa Đội trưởng. Bạn không đủ quyền hạn.";
            public const string MaxMembersReached = "Đội đã đạt giới hạn số lượng thành viên.";
            public const string MinMembersRequired = "Đội cần đủ số lượng thành viên tối thiểu.";
            public const string ApprovedTeamMinMembersRequired = "Đội đã được duyệt. Cần giữ đủ số lượng thành viên tối thiểu.";
            public const string StudentCodeAlreadyUsedInEvent = "Mã sinh viên đã tồn tại trong một đội khác cùng Event.";
            public const string EmailAlreadyUsedInEvent = "Email này đã được đăng ký trong một đội khác cùng Event.";
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
            public const string RoundNotActive = "Vòng thi hiện không ở trạng thái nhận bài.";
            public const string RoundNotStarted = "Vòng thi chưa bắt đầu.";
            public const string TeamNotQualifiedForRound = "Đội thi này chưa đủ điều kiện tham gia Round này.";
        }

        public static class Common
        {
            public const string InvalidStatus = "Status không hợp lệ.";
            public const string InvalidPageNumber = "PageNumber phải lớn hơn hoặc bằng 1.";
            public const string InvalidPageSize = "PageSize không hợp lệ.";
            public const string InvalidAccount = "Tài khoản của bạn không tồn tại hoặc đã bị vô hiệu hóa.";
            public const string InvalidRoundId = "RoundId không hợp lệ.";
            public const string InvalidTeamId = "TeamId không hợp lệ.";
            public const string InvalidSubmissionId = "SubmissionId không hợp lệ.";
            public const string InvalidMentorId = "MentorId không hợp lệ.";
            public const string RoundNotFound = "Không tìm thấy vòng thi.";
            public const string TrackNotFound = "Không tìm thấy hạng mục.";
            public const string MentorNotFound = "Không tìm thấy Mentor.";
            public const string InvalidEventId = "EventId không hợp lệ.";
            public const string InvalidJudgeId = "JudgeId không hợp lệ.";
            public const string JudgeNotFound = "Không tìm thấy Judge.";
            public const string JudgeNotInEvent = "Tài khoản này chưa được phân quyền Judge trong Event của Round này.";
        }

        public static class Round
        {
            public const string JudgeAlreadyAssigned = "Judge này đã được phân công vào Round này.";
            public const string NoTopicToAssign = "Không có Topic nào trong Round này để gán cho các nhóm.";
            public const string NotEnoughTopics = "Không đủ Topic để gán đề cho Round này.";
            public const string EventMustBeActiveToStartRound = "Chỉ được bắt đầu Round khi Event đang Active.";
            public const string PreviousRoundRankingRequired = "Chưa có kết quả xếp hạng vòng trước để xác định đội đi tiếp.";
            public const string NoTeamQualifiedForRound = "Không có đội đủ điều kiện tham gia Round này.";
        }

        public static class Event
        {
            public const string CannotReturnToRegistration = "Event đã Active thì không thể chuyển lại Registration.";
            public const string OnlyOneCurrentEventAllowed = "Chỉ được có một Event đang Registration hoặc Active tại một thời điểm.";
            public const string CurrentEventNotFound = "Hiện tại không có Event nào đang mở đăng ký hoặc đang diễn ra.";
        }

        public static class Criterion
        {
            public const string NotFound = "Không tìm thấy tiêu chí chấm điểm.";
            public const string NameRequired = "Tên tiêu chí không được để trống.";
            public const string MaxScoreInvalid = "Điểm tối đa phải lớn hơn 0.";
            public const string WeightInvalid = "Trọng số phải nằm trong khoảng 1 đến 100.";
            public const string WeightTotalExceeded = "Tổng trọng số của Round không được vượt quá 100.";
            public const string NameDuplicatedInRound = "Tên tiêu chí đã tồn tại trong Round này.";
            public const string AlreadyUsedByScore = "Tiêu chí đã được dùng để chấm điểm, không thể chỉnh sửa hoặc xóa.";
            public const string NotBelongToRound = "Tiêu chí không thuộc Round này.";
            public const string TemplateHasNoCriteria = "Template này chưa có tiêu chí để import.";
            public const string TemplateImportDuplicatedName = "Một hoặc nhiều tiêu chí trong template đã tồn tại trong Round này.";
        }

        public static class Score
        {
            public const string NotFound = "Không tìm thấy điểm chấm.";
            public const string SubmissionNotFound = "Không tìm thấy bài nộp.";
            public const string CriterionNotFound = "Không tìm thấy tiêu chí chấm điểm.";
            public const string RoundNotFound = "Không tìm thấy vòng thi.";
            public const string TrackNotFound = "Không tìm thấy hạng mục.";
            public const string SubmissionDisqualified = "Bài nộp đã bị loại, không thể chấm điểm.";
            public const string SubmissionDisqualifiedCannotUpdate = "Bài nộp đã bị loại, không thể cập nhật điểm.";
            public const string CriterionNotInSubmissionRound = "Tiêu chí không thuộc vòng thi của bài nộp này.";
            public const string RoundNotInScoring = "Chỉ được chấm điểm khi vòng thi đang ở trạng thái Scoring.";
            public const string JudgeNotActiveInEvent = "Tài khoản Judge không còn hoạt động trong Event của vòng thi này.";
            public const string JudgeNotAssignedToRound = "Bạn không được phân công chấm vòng thi này.";
            public const string JudgeNoUpdatePermission = "Bạn chỉ được chỉnh sửa điểm do chính bạn đã chấm.";
            public const string AlreadyScored = "Bạn đã chấm tiêu chí này cho bài nộp này.";
            public const string InvalidScoreRange = "Điểm chấm không hợp lệ.";
            public const string TeamDisqualified = "Đội thi đã bị loại, không thể chấm điểm.";
            public const string TeamDisqualifiedCannotUpdate = "Đội thi đã bị loại, không thể cập nhật điểm.";
        }

        public static class Ranking
        {
            public const string NotFound = "Không tìm thấy kết quả xếp hạng.";
        }
    }
}
