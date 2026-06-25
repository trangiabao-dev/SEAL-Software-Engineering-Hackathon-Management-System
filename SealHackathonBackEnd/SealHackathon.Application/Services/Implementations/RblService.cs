using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SealHackathon.Application.DTOs.Rbl;
using SealHackathon.Application.Services.Interfaces;
using SealHackathon.Domain.Entities;
using SealHackathon.Domain.Exceptions;
using SealHackathon.Domain.Interfaces.Repositories;

namespace SealHackathon.Application.Services.Implementations
{
    public class RblService : IRblService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RblService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<byte[]> ExportAnonymousScoresCsvAsync(int eventId)
        {
            var eventExists = await _unitOfWork.GetRepository<Event>()
                .GetFirstOrDefaultAsync(e => e.Id == eventId && !e.IsDeleted) != null;

            if (!eventExists)
            {
                throw new NotFoundException("Sự kiện không tồn tại hoặc đã bị xóa.");
            }

            var scoreRecords = await _unitOfWork
                .GetRepository<ScoreRecord>()
                .GetAllWithIncludeAsync(
                    sr => sr.Submission.Round.Track.EventId == eventId && !sr.Submission.Round.Track.IsDeleted,
                    sr => sr.Submission.Team,
                    sr => sr.Submission.Round,
                    sr => sr.Submission.Round.Track,
                    sr => sr.Judge,
                    sr => sr.Criterion
                );

            if (!scoreRecords.Any())
            {
                var emptyHeader = "SubmissionId,TeamCode,TrackName,RoundName,JudgeCode,CriterionName,Score,Weight,ScoredAt\n";
                return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(emptyHeader)).ToArray();
            }

            // Anonymize Teams and Judges
            var distinctTeamIds = scoreRecords
                .Select(sr => sr.Submission.TeamId)
                .Distinct()
                .OrderBy(id => id)
                .ToList();

            var teamCodeMap = distinctTeamIds
                .Select((id, index) => new { id, code = $"Team_{index + 1:D2}" })
                .ToDictionary(x => x.id, x => x.code);

            var distinctJudgeIds = scoreRecords
                .Select(sr => sr.JudgeId)
                .Distinct()
                .OrderBy(id => id)
                .ToList();

            var judgeCodeMap = distinctJudgeIds
                .Select((id, index) => new { id, code = $"Judge_{index + 1:D2}" })
                .ToDictionary(x => x.id, x => x.code);

            var sb = new StringBuilder();
            sb.AppendLine("SubmissionId,TeamCode,TrackName,RoundName,JudgeCode,CriterionName,Score,Weight,ScoredAt");

            foreach (var record in scoreRecords)
            {
                var submissionId = record.SubmissionId.ToString();
                var teamCode = teamCodeMap[record.Submission.TeamId];
                var trackName = EscapeCsvField(record.Submission.Round.Track.Name);
                var roundName = EscapeCsvField(record.Submission.Round.Name);
                var judgeCode = judgeCodeMap[record.JudgeId];
                var criterionName = EscapeCsvField(record.Criterion.Name);
                var score = record.Score;
                var weight = record.Criterion.Weight;
                var scoredAt = record.ScoredAt.ToString("yyyy-MM-dd HH:mm:ss");

                sb.AppendLine($"{submissionId},{teamCode},{trackName},{roundName},{judgeCode},{criterionName},{score},{weight},{scoredAt}");
            }

            var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
            var bom = Encoding.UTF8.GetPreamble(); // UTF-8 BOM
            return bom.Concat(csvBytes).ToArray();
        }

        public async Task<List<CriterionVarianceResponse>> GetCriteriaVarianceAsync(int eventId)
        {
            var eventExists = await _unitOfWork.GetRepository<Event>()
                .GetFirstOrDefaultAsync(e => e.Id == eventId && !e.IsDeleted) != null;

            if (!eventExists)
            {
                throw new NotFoundException("Sự kiện không tồn tại hoặc đã bị xóa.");
            }

            // Get all criteria in this event
            var criteria = await _unitOfWork
                .GetRepository<Criterion>()
                .GetAllWithIncludeAsync(
                    c => c.Round.Track.EventId == eventId && !c.Round.Track.IsDeleted,
                    c => c.Round,
                    c => c.Round.Track
                );

            if (!criteria.Any())
            {
                return new List<CriterionVarianceResponse>();
            }

            var scoreRecords = await _unitOfWork
                .GetRepository<ScoreRecord>()
                .GetAllWithIncludeAsync(
                    sr => sr.Submission.Round.Track.EventId == eventId && !sr.Submission.Round.Track.IsDeleted,
                    sr => sr.Submission
                );

            var recordsByCriterion = scoreRecords
                .GroupBy(sr => sr.CriterionId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var responseList = new List<CriterionVarianceResponse>();

            foreach (var criterion in criteria)
            {
                double avgVariance = 0;
                var submissionVariances = new List<double>();

                if (recordsByCriterion.TryGetValue(criterion.Id, out var records))
                {
                    var groupedBySubmission = records.GroupBy(r => r.SubmissionId);

                    foreach (var submissionGroup in groupedBySubmission)
                    {
                        var scores = submissionGroup.Select(r => r.Score).ToList();
                        if (scores.Count >= 2)
                        {
                            double variance = CalculateSampleVariance(scores);
                            submissionVariances.Add(variance);
                        }
                    }
                }

                if (submissionVariances.Any())
                {
                    avgVariance = Math.Round(submissionVariances.Average(), 3);
                }

                responseList.Add(new CriterionVarianceResponse
                {
                    CriterionId = criterion.Id,
                    CriterionName = criterion.Name,
                    RoundName = criterion.Round.Name,
                    TrackName = criterion.Round.Track.Name,
                    Variance = avgVariance,
                    SubmissionsCount = submissionVariances.Count
                });
            }

            // Sort by Track Name, then Round OrderIndex, then Criterion Name
            return responseList
                .OrderBy(r => r.TrackName)
                .ThenBy(r => r.RoundName)
                .ThenBy(r => r.CriterionName)
                .ToList();
        }

        private static double CalculateSampleVariance(List<double> values)
        {
            if (values.Count < 2) return 0;
            double mean = values.Average();
            double sumOfSquares = values.Sum(v => Math.Pow(v - mean, 2));
            return sumOfSquares / (values.Count - 1);
        }

        private static string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field)) return string.Empty;
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
            {
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }
            return field;
        }
    }
}
