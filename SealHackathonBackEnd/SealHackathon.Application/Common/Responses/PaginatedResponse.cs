using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SealHackathon.Application.Common.Responses
{
    public class PaginatedResponse<T>
    {
        // Danh sách data của trang hiện tại
        public List<T> Items { get; set; } = new List<T>();

        // Trang hiện tại
        public int PageNumber { get; set; }

        // Số bản ghi mỗi trang
        public int PageSize { get; set; }

        // Tổng số bản ghi trong DB
        public int TotalRecords { get; set; }

        // Tổng số trang — tự động tính từ TotalRecords và PageSize
        // Math.Ceiling: làm tròn lên — 11 bản ghi / 10 mỗi trang = 1.1 → 2 trang
        public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);

        // Có trang trước không?
        public bool HasPreviousPage => PageNumber > 1;

        // Có trang sau không?
        public bool HasNextPage => PageNumber < TotalPages;

        public PaginatedResponse(List<T> items, int totalRecords, int pageNumber, int pageSize)
        {
            Items = items;
            TotalRecords = totalRecords;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }
    }
}