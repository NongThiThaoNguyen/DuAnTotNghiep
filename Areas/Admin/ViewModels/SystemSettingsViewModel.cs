using System.ComponentModel.DataAnnotations;

namespace DuAnTotNghiep.Areas.Admin.ViewModels
{
    public class SystemSettingsViewModel
    {
        // General Tab
        [Required(ErrorMessage = "Tên website là bắt buộc")]
        [Display(Name = "Tên website")]
        public string SiteName { get; set; } = "AI Learn";

        [Display(Name = "Email liên hệ")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? ContactEmail { get; set; } = "";

        [Display(Name = "Số điện thoại liên hệ")]
        public string? ContactPhone { get; set; } = "";

        // AI Model Tab
        [Display(Name = "Mô hình AI mặc định")]
        public string? DefaultAiModel { get; set; } = "gpt-4o-mini";

        [Display(Name = "Giới hạn Tokens")]
        public string? MaxTokens { get; set; } = "2000";

        // Email Tab (SMTP)
        [Display(Name = "Máy chủ SMTP")]
        public string? SmtpServer { get; set; } = "";

        [Display(Name = "Cổng SMTP")]
        public string? SmtpPort { get; set; } = "587";

        [Display(Name = "Tài khoản SMTP")]
        public string? SmtpUsername { get; set; } = "";
        
        [Display(Name = "Mật khẩu SMTP")]
        public string? SmtpPassword { get; set; } = "";
    }
}
