using System;
using System.Collections.Generic;

namespace SealHackathon.Application.DTOs.Submission
{
    public class ImportSubmissionsRequest
    {
        public bool AutoCreateRoundTeam { get; set; } = true;
        public List<ImportSubmissionDto> Submissions { get; set; } = new List<ImportSubmissionDto>();
    }

    public class ImportSubmissionDto
    {
        public int RowNumber { get; set; }
        public Guid TeamId { get; set; }
        public int? TopicId { get; set; }
        public string PresentationUrl { get; set; }
    }

    public class ImportSubmissionSuccessDto
    {
        public int RowNumber { get; set; }
        public Guid SubmissionId { get; set; }
        public Guid TeamId { get; set; }
        public int RoundId { get; set; }
    }
}
