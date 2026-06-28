using System;
using System.Threading.Tasks;
using DuAnTotNghiep.Enums;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Services.Interfaces;

namespace DuAnTotNghiep.Services
{
    public class ValidateLicenseService : IValidateLicenseService
    {
        public Task ValidateLicenseAsync(ReferenceSource source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            bool isExternal = source.SourceType == ReferenceSourceType.OFFICIAL ||
                              source.SourceType == ReferenceSourceType.OPEN_LICENSE ||
                              source.SourceType == ReferenceSourceType.REFERENCE_ONLY;

            if (isExternal)
            {
                if (string.IsNullOrWhiteSpace(source.LicenseNote))
                {
                    throw new InvalidOperationException("Ghi chú bản quyền (License Note) là bắt buộc đối với nguồn tham khảo bên ngoài (External).");
                }

                if (source.LicenseNote.Length < 10)
                {
                    throw new InvalidOperationException("Ghi chú bản quyền (License Note) phải có độ dài từ 10 ký tự trở lên.");
                }

                if (!source.UsagePolicy.HasValue)
                {
                    throw new InvalidOperationException("Chính sách sử dụng (Usage Policy) là bắt buộc đối với nguồn tham khảo bên ngoài.");
                }

                var policy = source.UsagePolicy.Value;
                if (policy != ReferenceUsagePolicy.REFERENCE_ONLY &&
                    policy != ReferenceUsagePolicy.OPEN_LICENSE &&
                    policy != ReferenceUsagePolicy.RESTRICTED)
                {
                    throw new InvalidOperationException("Chính sách sử dụng không hợp lệ.");
                }

                // Không cho set open_license nếu chưa verify (tức là thiếu compliance_evidence_url)
                if (policy == ReferenceUsagePolicy.OPEN_LICENSE)
                {
                    if (string.IsNullOrWhiteSpace(source.ComplianceEvidenceUrl))
                    {
                        throw new InvalidOperationException("Không được phép chọn Giấy phép mở (Open License) nếu chưa cung cấp URL minh chứng xác minh bản quyền.");
                    }

                    if (!Uri.TryCreate(source.ComplianceEvidenceUrl, UriKind.Absolute, out var uriResult) ||
                        (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
                    {
                        throw new InvalidOperationException("Địa chỉ URL minh chứng xác minh bản quyền không hợp lệ.");
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}
