using DuAnTotNghiep.Models;
using DuAnTotNghiep.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace DuAnTotNghiep.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MigrationPatchController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MigrationPatchController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("run-insert-students")]
        public async Task<IActionResult> RunInsertStudents()
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleCode == "STUDENT");
            if (role == null)
            {
                return BadRequest("Role STUDENT not found.");
            }

            // Tạo hash duy nhất cho 100 học viên để tăng tốc độ
            var passwordHash = BCrypt.Net.BCrypt.HashPassword("Password@123");
            
            int successCount = 0;
            int skipCount = 0;

            for (int i = 1; i <= 100; i++)
            {
                var username = $"student{i:D3}";
                var email = $"{username}@test.com";

                var exists = await _context.Users.AnyAsync(u => u.Email == email);
                if (!exists)
                {
                    var user = new User
                    {
                        FullName = $"Student {i}",
                        Email = email,
                        RoleId = role.Id,
                        Status = "ACTIVE",
                        PasswordHash = passwordHash,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    var auditLog = new AuditLog
                    {
                        UserId = user.Id,
                        Action = "REGISTER",
                        EntityName = "User",
                        EntityId = user.Id,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.AuditLogs.Add(auditLog);
                    await _context.SaveChangesAsync();

                    successCount++;
                }
                else
                {
                    skipCount++;
                }
            }

            return Ok(new { 
                Message = "Migration complete",
                Inserted = successCount, 
                Skipped = skipCount 
            });
        }
    }
}
