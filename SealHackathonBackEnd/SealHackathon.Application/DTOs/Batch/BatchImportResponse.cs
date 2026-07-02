using System.Collections.Generic;

namespace SealHackathon.Application.DTOs.Batch
{
    public class BatchImportResponse<TUpdated, TCreated>
    {
        public bool Success { get; set; } = true;
        public string Message { get; set; }
        public BatchImportData<TUpdated, TCreated> Data { get; set; } = new BatchImportData<TUpdated, TCreated>();
        public object Errors { get; set; } = null;
    }

    public class BatchImportData<TUpdated, TCreated>
    {
        public List<TCreated> Created { get; set; } = new List<TCreated>();
        public List<TUpdated> Updated { get; set; } = new List<TUpdated>();
        public List<BatchImportFailedDto> Failed { get; set; } = new List<BatchImportFailedDto>();
    }

    public class BatchImportFailedDto
    {
        public int RowNumber { get; set; }
        public string Reason { get; set; }
    }
}
