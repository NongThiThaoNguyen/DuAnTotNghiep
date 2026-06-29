using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace DuAnTotNghiep.ViewModels.Profile;

public class AvatarUploadViewModel
{
    [Required(ErrorMessage = "Vui lòng chọn file ảnh")]
    [Display(Name = "Ảnh đại diện")]
    public IFormFile AvatarFile { get; set; } = null!;
}
