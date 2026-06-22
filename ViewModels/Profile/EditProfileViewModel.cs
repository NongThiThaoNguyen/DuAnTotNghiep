using System;
using System.ComponentModel.DataAnnotations;

namespace DuAnTotNghiep.ViewModels.Profile;

public class EditProfileViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
    [MaxLength(255)]
    [Display(Name = "Họ và tên")]
    public string FullName { get; set; } = null!;

    [MaxLength(20)]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    [Display(Name = "Số điện thoại")]
    public string? Phone { get; set; }

    [Display(Name = "Ngày sinh")]
    public DateOnly? DateOfBirth { get; set; }

    [MaxLength(20)]
    [Display(Name = "Giới tính")]
    public string? Gender { get; set; }

    [MaxLength(100)]
    [Display(Name = "Quốc gia")]
    public string? Country { get; set; }

    [MaxLength(500)]
    [Display(Name = "Giới thiệu bản thân")]
    public string? Bio { get; set; }
}
