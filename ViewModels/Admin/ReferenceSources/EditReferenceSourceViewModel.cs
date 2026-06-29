using DuAnTotNghiep.Enums;
using System.ComponentModel.DataAnnotations;

namespace DuAnTotNghiep.ViewModels.Admin.ReferenceSources
{
    public class EditReferenceSourceViewModel
    {
        [Required(ErrorMessage = "Id nguồn tham khảo là bắt buộc.")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên nguồn tham khảo là bắt buộc.")]
        [StringLength(255, ErrorMessage = "Tên nguồn tham khảo không được vượt quá 255 ký tự.")]
        [Display(Name = "Tên nguồn tham khảo")]
        public string SourceName { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "URL nguồn không được vượt quá 1000 ký tự.")]
        [Url(ErrorMessage = "Địa chỉ URL không đúng định dạng HTTP hoặc HTTPS.")]
        [Display(Name = "Địa chỉ URL")]
        public string? SourceUrl { get; set; }

        [StringLength(1000, ErrorMessage = "URL bằng chứng tuân thủ không được vượt quá 1000 ký tự.")]
        [Url(ErrorMessage = "URL bằng chứng tuân thủ không đúng định dạng HTTP hoặc HTTPS.")]
        [Display(Name = "URL bằng chứng tuân thủ")]
        public string? ComplianceEvidenceUrl { get; set; }

        [Required(ErrorMessage = "Loại nguồn tham khảo là bắt buộc.")]
        [Display(Name = "Loại nguồn tham khảo")]
        public ReferenceSourceType SourceType { get; set; }

        [StringLength(255, ErrorMessage = "Tên tác giả không được vượt quá 255 ký tự.")]
        [Display(Name = "Tác giả")]
        public string? Author { get; set; }

        [StringLength(255, ErrorMessage = "Tên tổ chức không được vượt quá 255 ký tự.")]
        [Display(Name = "Tổ chức")]
        public string? Organization { get; set; }

        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Display(Name = "Ghi chú bản quyền / Giấy phép")]
        public string? LicenseNote { get; set; }

        [Display(Name = "Chính sách sử dụng")]
        public ReferenceUsagePolicy? UsagePolicy { get; set; }

        [Display(Name = "Trạng thái hiện tại")]
        public ReferenceReviewStatus Status { get; set; }

        [Display(Name = "Cho phép hoạt động")]
        public bool IsActive { get; set; }
    }
}
