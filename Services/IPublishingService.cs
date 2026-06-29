using DuAnTotNghiep.Models.DTOs;
using System.Threading;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services
{
    public interface IPublishingService
    {
        /// <summary>
        /// Publish AI-generated content to Question Bank
        /// </summary>
        /// <param name="aiContentId">ID of AiGeneratedContent (must be APPROVED)</param>
        /// <param name="reviewedByUserId">User ID performing the publish action</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>PublishResult with question_id, quiz_id (if created), and message</returns>
        Task<PublishResult> PublishToQuestionBankAsync(int aiContentId, int reviewedByUserId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Publish entire batch of approved content to Question Bank + optionally create a quiz draft
        /// </summary>
        /// <param name="batchId">Batch ID to publish</param>
        /// <param name="publishedByUserId">User ID performing the action</param>
        /// <param name="createQuizDraft">If true, create a quiz with all published questions</param>
        /// <param name="quizTitle">Title for the draft quiz</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>PublishBatchResult with counts and IDs</returns>
        Task<PublishBatchResult> PublishBatchAsync(string batchId, int publishedByUserId, bool createQuizDraft = false, string quizTitle = null, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Result of publishing a single AI content to Question Bank
    /// </summary>
    public class PublishResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public int? PublishedQuestionId { get; set; }
        public int? CreatedQuizId { get; set; }
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// Result of publishing an entire batch
    /// </summary>
    public class PublishBatchResult
    {
        public bool IsSuccess { get; set; }
        public int PublishedCount { get; set; }
        public int FailureCount { get; set; }
        public int? CreatedQuizId { get; set; }
        public string Message { get; set; }
        public string ErrorMessage { get; set; }
    }
}
