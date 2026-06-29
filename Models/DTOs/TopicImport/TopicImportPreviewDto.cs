using System.Collections.Generic;

namespace DuAnTotNghiep.Models.DTOs.TopicImport
{
    public class TopicImportErrorDto
    {
        public int RowNumber { get; set; }
        public string ColumnName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class TopicImportPreviewDto
    {
        public int TotalRows { get; set; }
        public int ValidRows { get; set; }
        public int ErrorRows { get; set; }
        public List<TopicImportErrorDto> Errors { get; set; } = new();
        public List<TopicImportRowDto> ValidRowsData { get; set; } = new();
    }
}
