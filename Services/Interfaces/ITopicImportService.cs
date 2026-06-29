using System.Collections.Generic;
using System.Threading.Tasks;
using DuAnTotNghiep.Models.DTOs.TopicImport;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface ITopicImportService
    {
        /// <summary>
        /// Preview import file without persisting data.
        /// Returns total rows, valid rows, error rows and detailed error list.
        /// </summary>
        Task<TopicImportPreviewDto> PreviewImportAsync(byte[] fileBytes, string fileName);

        /// <summary>
        /// Validate the file content fully. Returns true if valid, otherwise throws ValidationException.
        /// </summary>
        Task ValidateImportFileAsync(byte[] fileBytes, string fileName);

        /// <summary>
        /// Perform the actual import (create / update) within a transaction.
        /// </summary>
        Task ImportTopicsAsync(byte[] fileBytes, string fileName);

        /// <summary>
        /// Seed default demo topics (idempotent).
        /// </summary>
        Task SeedDefaultTopicsAsync();
    }
}
