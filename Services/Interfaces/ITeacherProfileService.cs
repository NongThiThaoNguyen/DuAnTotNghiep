using DuAnTotNghiep.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface ITeacherProfileService
    {
        Task<User?> GetTeacherProfileAsync(int teacherId);
        Task UpdateTeacherProfileAsync(int teacherId, User model, IFormFile? avatarFile, string? bio, string? gender, string? country, DateOnly? dateOfBirth);
    }
}
