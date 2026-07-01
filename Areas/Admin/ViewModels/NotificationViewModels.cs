using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DuAnTotNghiep.Models;

namespace DuAnTotNghiep.Areas.Admin.ViewModels
{
    public class NotificationListViewModel
    {
        public List<Notification> Notifications { get; set; } = new List<Notification>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
    }

    public class CreateNotificationViewModel
    {
        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        [StringLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nội dung không được để trống")]
        [StringLength(2000, ErrorMessage = "Nội dung không được vượt quá 2000 ký tự")]
        public string Content { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn loại thông báo")]
        public string NotificationType { get; set; } = "INFO"; // SYSTEM, INFO, WARNING, URGENT

        [Required(ErrorMessage = "Vui lòng chọn đối tượng nhận")]
        public string TargetType { get; set; } = "ALL"; // ALL, STUDENTS, TEACHERS, SPECIFIC_USER

        public int? TargetUserId { get; set; }
    }
}
