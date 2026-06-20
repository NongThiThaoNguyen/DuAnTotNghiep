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
            await SeedAdminAsync();
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

        private async Task SeedAdminAsync()
        {
            var adminEmail = "admin@aistudyenglish.com";

            // Kiểm tra xem đã có admin chưa
            if (!await _userRepository.ExistsByEmailAsync(adminEmail))
            {
                // Lấy Role ADMIN
                var adminRole = await _roleRepository.GetByCodeAsync("ADMIN");
                if (adminRole == null) return; // Nếu chưa có role admin thì bỏ qua

                var newAdmin = new User
                {
                    Email = adminEmail,
                    PasswordHash = PasswordHelper.HashPassword("Admin@123"),
                    FullName = "System Administrator",
                    RoleId = adminRole.Id,
                    Status = "ACTIVE",
                    CreatedAt = DateTime.UtcNow,
                    FailedLoginCount = 0
                };

                await _userRepository.AddAsync(newAdmin);
                await _userRepository.SaveChangesAsync();
            }
        }
    }
}
