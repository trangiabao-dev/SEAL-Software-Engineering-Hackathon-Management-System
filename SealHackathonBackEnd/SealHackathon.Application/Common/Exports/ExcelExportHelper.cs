using ClosedXML.Excel;

namespace SealHackathon.Application.Common.Exports
{
    /// <summary>
    /// Tạo file XLSX dạng bảng từ header, dữ liệu và định dạng cột được cung cấp.
    /// </summary>
    public static class ExcelExportHelper
    {
        private const double MaximumColumnWidth = 45;

        /// <summary>
        /// Tạo workbook một sheet và trả về nội dung file dưới dạng byte array.
        /// </summary>
        public static byte[] CreateWorkbook(
            string sheetName,
            IReadOnlyList<string> headers,
            IReadOnlyList<object?[]> rows,
            IReadOnlyDictionary<int, string>? numberFormats = null)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(sheetName);

            for (var column = 0; column < headers.Count; column++)
                worksheet.Cell(1, column + 1).Value = headers[column];

            for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                var values = rows[rowIndex];
                for (var column = 0; column < values.Length; column++)
                    SetCellValue(worksheet, rowIndex + 2, column + 1, values[column]);
            }

            var usedRange = worksheet.Range(1, 1, rows.Count + 1, headers.Count);
            usedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            usedRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            usedRange.SetAutoFilter();

            var headerRange = worksheet.Range(1, 1, 1, headers.Count);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.DarkBlue;
            headerRange.Style.Font.FontColor = XLColor.White;

            worksheet.SheetView.FreezeRows(1);

            if (numberFormats is not null)
            {
                foreach (var numberFormat in numberFormats)
                    worksheet.Column(numberFormat.Key).Style.NumberFormat.Format = numberFormat.Value;
            }

            worksheet.Columns(1, headers.Count).AdjustToContents();
            foreach (var column in worksheet.Columns(1, headers.Count))
            {
                if (column.Width > MaximumColumnWidth)
                    column.Width = MaximumColumnWidth;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        // =============== Private helpers ===============

        /// <summary>
        /// Ghi giá trị vào cell theo đúng kiểu dữ liệu mà ClosedXML hỗ trợ.
        /// </summary>
        private static void SetCellValue(
            IXLWorksheet worksheet,
            int row,
            int column,
            object? value)
        {
            var cell = worksheet.Cell(row, column);

            switch (value)
            {
                case null:
                    cell.Value = string.Empty;
                    break;
                case Guid guidValue:
                    cell.Value = guidValue.ToString();
                    break;
                case decimal decimalValue:
                    cell.Value = decimalValue;
                    break;
                case double doubleValue:
                    cell.Value = doubleValue;
                    break;
                case int intValue:
                    cell.Value = intValue;
                    break;
                case bool boolValue:
                    cell.Value = boolValue;
                    break;
                case DateTime dateTimeValue:
                    cell.Value = dateTimeValue;
                    break;
                default:
                    cell.Value = value.ToString();
                    break;
            }
        }
    }
}
