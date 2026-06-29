using System;

namespace DuAnTotNghiep.ViewModels
{
    public class PersistenceResultDTO
    {
        public bool IsSuccess { get; set; }
        public int? AnalysisId { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime SavedAt { get; set; }

        public static PersistenceResultDTO Success(int id) => new()
        {
            IsSuccess = true,
            AnalysisId = id,
            SavedAt = DateTime.UtcNow
        };

        public static PersistenceResultDTO Fail(string msg) => new()
        {
            IsSuccess = false,
            ErrorMessage = msg,
            SavedAt = DateTime.UtcNow
        };
    }
}
