using System;
using System.Collections.Generic;
using System.Linq;
using DuAnTotNghiep.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DuAnTotNghiep.Helpers
{
    public static class ReferenceHelper
    {
        public static string GetSourceTypeLabel(ReferenceSourceType type)
        {
            return type switch
            {
                ReferenceSourceType.OFFICIAL => "Chính thống (Official)",
                ReferenceSourceType.OPEN_LICENSE => "Giấy phép mở (Open License)",
                ReferenceSourceType.SELF_CREATED => "Tự biên soạn (Self Created)",
                ReferenceSourceType.TEACHER_CREATED => "Giáo viên soạn (Teacher Created)",
                ReferenceSourceType.REFERENCE_ONLY => "Chỉ tham khảo (Reference Only)",
                _ => type.ToString()
            };
        }

        public static string GetReviewStatusLabel(ReferenceReviewStatus status)
        {
            return status switch
            {
                ReferenceReviewStatus.DRAFT => "Nháp (Draft)",
                ReferenceReviewStatus.PENDING => "Chờ duyệt (Pending)",
                ReferenceReviewStatus.APPROVED => "Đã duyệt (Approved)",
                ReferenceReviewStatus.REJECTED => "Từ chối (Rejected)",
                ReferenceReviewStatus.ARCHIVED => "Lưu trữ (Archived)",
                _ => status.ToString()
            };
        }

        public static string GetUsagePolicyLabel(ReferenceUsagePolicy? policy)
        {
            if (!policy.HasValue) return "Không xác định";
            return policy.Value switch
            {
                ReferenceUsagePolicy.REFERENCE_ONLY => "Chỉ tham khảo (Reference Only)",
                ReferenceUsagePolicy.OPEN_LICENSE => "Giấy phép mở (Open License)",
                ReferenceUsagePolicy.RESTRICTED => "Hạn chế sử dụng (Restricted)",
                _ => policy.Value.ToString()
            };
        }

        public static string GetReviewStatusBadgeClass(ReferenceReviewStatus status)
        {
            return status switch
            {
                ReferenceReviewStatus.DRAFT => "bg-secondary text-white",
                ReferenceReviewStatus.PENDING => "bg-warning text-dark",
                ReferenceReviewStatus.APPROVED => "bg-success text-white",
                ReferenceReviewStatus.REJECTED => "bg-danger text-white",
                ReferenceReviewStatus.ARCHIVED => "bg-dark text-white",
                _ => "bg-light text-dark"
            };
        }

        public static List<SelectListItem> GetSourceTypeSelectList(ReferenceSourceType? selected = null)
        {
            return Enum.GetValues(typeof(ReferenceSourceType))
                .Cast<ReferenceSourceType>()
                .Select(t => new SelectListItem
                {
                    Value = t.ToString(),
                    Text = GetSourceTypeLabel(t),
                    Selected = selected.HasValue && selected.Value == t
                })
                .ToList();
        }

        public static List<SelectListItem> GetReviewStatusSelectList(ReferenceReviewStatus? selected = null)
        {
            return Enum.GetValues(typeof(ReferenceReviewStatus))
                .Cast<ReferenceReviewStatus>()
                .Select(s => new SelectListItem
                {
                    Value = s.ToString(),
                    Text = GetReviewStatusLabel(s),
                    Selected = selected.HasValue && selected.Value == s
                })
                .ToList();
        }

        public static List<SelectListItem> GetUsagePolicySelectList(ReferenceUsagePolicy? selected = null)
        {
            return Enum.GetValues(typeof(ReferenceUsagePolicy))
                .Cast<ReferenceUsagePolicy>()
                .Select(p => new SelectListItem
                {
                    Value = p.ToString(),
                    Text = GetUsagePolicyLabel(p),
                    Selected = selected.HasValue && selected.Value == p
                })
                .ToList();
        }
    }
}
