using DuAnTotNghiep.Enums;
using DuAnTotNghiep.Repositories.Interfaces;
using DuAnTotNghiep.Services.Interfaces;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services
{
    public class ReferenceSourcePolicyService : IReferenceSourcePolicyService
    {
        private readonly IReferenceSourceRepository _repository;

        public ReferenceSourcePolicyService(IReferenceSourceRepository repository)
        {
            _repository = repository;
        }

        public async Task<SourceValidationResult> ValidateSourceUsableAsync(int sourceId)
        {
            return await CheckSourceBeforeUseAsync(sourceId);
        }

        public async Task<SourceValidationResult> CheckSourceBeforeUseAsync(int sourceId)
        {
            var source = await _repository.GetDetailsAsync(sourceId);
            if (source == null)
            {
                return new SourceValidationResult
                {
                    IsValid = false,
                    Level = "BLOCKED",
                    Message = "Nguồn tham khảo không tồn tại."
                };
            }

            switch (source.Status)
            {
                case ReferenceReviewStatus.APPROVED:
                    return new SourceValidationResult
                    {
                        IsValid = true,
                        Level = "OK",
                        Message = "Nguồn tham khảo hợp lệ."
                    };

                case ReferenceReviewStatus.PENDING:
                    return new SourceValidationResult
                    {
                        IsValid = true,
                        Level = "WARNING",
                        Message = "Nguồn chưa được kiểm duyệt (Source not reviewed)."
                    };

                case ReferenceReviewStatus.REJECTED:
                    return new SourceValidationResult
                    {
                        IsValid = false,
                        Level = "BLOCKED",
                        Message = "Nguồn tham khảo đã bị từ chối (Source rejected)."
                    };

                case ReferenceReviewStatus.ARCHIVED:
                    return new SourceValidationResult
                    {
                        IsValid = false,
                        Level = "BLOCKED",
                        Message = "Nguồn tham khảo đã bị lưu trữ (Source archived)."
                    };

                case ReferenceReviewStatus.DRAFT:
                default:
                    return new SourceValidationResult
                    {
                        IsValid = false,
                        Level = "BLOCKED",
                        Message = "Nguồn tham khảo đang ở trạng thái Nháp (Draft)."
                    };
            }
        }
    }
}
