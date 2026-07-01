using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services
{
    public class TeacherProfileService : ITeacherProfileService
    {
        private readonly ApplicationDbContext _context;

        public TeacherProfileService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetTeacherProfileAsync(int teacherId)
        {
            return await _context.Users
                .Include(u => u.UserProfile)
                .FirstOrDefaultAsync(u => u.Id == teacherId);
        }

        public async Task UpdateTeacherProfileAsync(int teacherId, User model, IFormFile? avatarFile, string? bio, string? gender, string? country, DateOnly? dateOfBirth)
        {
            var existing = await _context.Users
                .Include(u => u.UserProfile)
                .FirstOrDefaultAsync(u => u.Id == teacherId);

            if (existing == null) throw new InvalidOperationException("Teacher not found");

            existing.FullName = model.FullName;
            existing.Email = model.Email;
            existing.Phone = model.Phone;

            // Handle file upload for Avatar
            if (avatarFile != null && avatarFile.Length > 0)
            {
                var extension = Path.GetExtension(avatarFile.FileName);
                var filename = Guid.NewGuid().ToString() + extension;
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var filePath = Path.Combine(uploadsFolder, filename);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await avatarFile.CopyToAsync(stream);
                }

                existing.AvatarUrl = "/uploads/avatars/" + filename;
            }

            // Handle UserProfile updates
            var profile = existing.UserProfile;
            if (profile == null)
            {
                profile = new UserProfile
                {
                    UserId = teacherId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.UserProfiles.Add(profile);
            }

            profile.Bio = bio;
            profile.Gender = gender;
            profile.Country = country;
            profile.DateOfBirth = dateOfBirth;
            profile.UpdatedAt = DateTime.UtcNow;

            existing.UpdatedAt = DateTime.UtcNow;

            _context.Update(existing);
            await _context.SaveChangesAsync();
        }
    }
}
