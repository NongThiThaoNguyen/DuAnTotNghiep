namespace DuAnTotNghiep.ViewModels
{
    public class PublishResultViewModel
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public int? PublishedQuestionId { get; set; }
        public int? CreatedQuizId { get; set; }
        public int AiContentId { get; set; }
        public string ErrorMessage { get; set; }
        
        // Links for user
        public string ViewQuestionLink { get; set; }
        public string ViewQuizLink { get; set; }
    }

    public class PublishBatchResultViewModel
    {
        public bool IsSuccess { get; set; }
        public int PublishedCount { get; set; }
        public int FailureCount { get; set; }
        public int? CreatedQuizId { get; set; }
        public string Message { get; set; }
        public string ErrorMessage { get; set; }
        public string BatchId { get; set; }
    }
}
