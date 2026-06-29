namespace DuAnTotNghiep.Models.Enums
{
    public enum ReferenceSourceType
    {
        OFFICIAL,
        OPEN_LICENSE,
        SELF_CREATED,
        TEACHER_CREATED,
        REFERENCE_ONLY
    }

    public enum ReferenceReviewStatus
    {
        DRAFT,
        PENDING,
        APPROVED,
        REJECTED,
        ARCHIVED
    }

    public enum ReferenceUsagePolicy
    {
        REFERENCE_ONLY,
        OPEN_LICENSE,
        RESTRICTED
    }
}
