using DuAnTotNghiep.Helpers;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Repositories.Interfaces;

namespace DuAnTotNghiep.Data.Seeders
{
    public class DatabaseSeeder
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRepository _userRepository;

        public DatabaseSeeder(IRoleRepository roleRepository, IUserRepository userRepository)
        {
            _roleRepository = roleRepository;
            _userRepository = userRepository;
        }

        public async Task SeedAsync()
        {
            await SeedRolesAsync();
            await SeedUsersAsync();
        }

        private async Task SeedRolesAsync()
        {
            var rolesToSeed = new List<Role>
            {
                new Role { RoleCode = "ADMIN", RoleName = "Quản trị viên", Description = "Quản trị viên toàn hệ thống", CreatedAt = DateTime.UtcNow },
                new Role { RoleCode = "TEACHER", RoleName = "Giáo viên", Description = "Giáo viên giảng dạy", CreatedAt = DateTime.UtcNow },
                new Role { RoleCode = "STUDENT", RoleName = "Học sinh", Description = "Học sinh tham gia hệ thống", CreatedAt = DateTime.UtcNow }
            };

            bool changesMade = false;

            foreach (var role in rolesToSeed)
            {
                if (!await _roleRepository.ExistsAsync(r => r.RoleCode == role.RoleCode))
                {
                    await _roleRepository.AddAsync(role);
                    changesMade = true;
                }
            }

            if (changesMade)
            {
                await _roleRepository.SaveChangesAsync();
            }
        }

        private async Task SeedUsersAsync()
        {
            var adminRole = await _roleRepository.GetByCodeAsync("ADMIN");
            var teacherRole = await _roleRepository.GetByCodeAsync("TEACHER");
            var studentRole = await _roleRepository.GetByCodeAsync("STUDENT");

            if (adminRole == null || teacherRole == null || studentRole == null) return;

            var defaultPassword = PasswordHelper.HashPassword("Password@123");

            var usersToSeed = new List<User>
            {
                // Admin Account
                new User { Email = "admin@aistudyenglish.com", PasswordHash = defaultPassword, FullName = "System Administrator", RoleId = adminRole.Id, Status = "ACTIVE", CreatedAt = DateTime.UtcNow, FailedLoginCount = 0 },
                
                // Teacher Account
                new User { Email = "teacher@aistudyenglish.com", PasswordHash = defaultPassword, FullName = "English Teacher", RoleId = teacherRole.Id, Status = "ACTIVE", CreatedAt = DateTime.UtcNow, FailedLoginCount = 0 },
                
                // Student Accounts
                new User { Email = "student1@aistudyenglish.com", PasswordHash = defaultPassword, FullName = "Student One", RoleId = studentRole.Id, Status = "ACTIVE", CreatedAt = DateTime.UtcNow, FailedLoginCount = 0 },
                new User { Email = "student2@aistudyenglish.com", PasswordHash = defaultPassword, FullName = "Student Two", RoleId = studentRole.Id, Status = "ACTIVE", CreatedAt = DateTime.UtcNow, FailedLoginCount = 0 },
                
                // Locked Account (dùng để test Lockout / Invalidation)
                new User { Email = "lockeduser@aistudyenglish.com", PasswordHash = defaultPassword, FullName = "Locked User", RoleId = studentRole.Id, Status = "LOCKED", CreatedAt = DateTime.UtcNow, FailedLoginCount = 0, LockoutUntil = DateTime.UtcNow.AddYears(100) },
                
                // Test OTP User
                new User { Email = "testotp@aistudyenglish.com", PasswordHash = defaultPassword, FullName = "Test OTP User", RoleId = studentRole.Id, Status = "ACTIVE", CreatedAt = DateTime.UtcNow, FailedLoginCount = 0 }
            };

            bool changesMade = false;

            foreach (var user in usersToSeed)
            {
                if (!await _userRepository.ExistsByEmailAsync(user.Email))
                {
                    await _userRepository.AddAsync(user);
                    changesMade = true;
                }
            }

            if (changesMade)
            {
                await _userRepository.SaveChangesAsync();
            }
        }
    }
}
