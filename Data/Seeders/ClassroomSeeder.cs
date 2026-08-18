using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Data.Seeders
{
    public class ClassroomSeeder
    {
        private readonly ApplicationDbContext _context;

        public ClassroomSeeder(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            if (await _context.Classrooms.AnyAsync()) return;

            // Find Teachers (User with Role TEACHER)
            var teachers = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.Role.RoleCode == "TEACHER" || u.RoleId == 2)
                .ToListAsync();

            if (!teachers.Any())
            {
                // Fallback to any user if no teacher role found
                teachers = await _context.Users.Take(3).ToListAsync();
            }

            if (!teachers.Any()) return;

            // Get Levels
            var levels = await _context.EnglishProficiencyLevels.Where(l => l.IsActive).ToListAsync();
            if (!levels.Any()) return;

            var classrooms = new List<Classroom>();
            int teacherIdx = 0;

            foreach (var level in levels)
            {
                var teacher = teachers[teacherIdx % teachers.Count];
                teacherIdx++;

                // Create Morning Class
                var morningClass = new Classroom
                {
                    ClassName = $"Lớp {level.Code} - Sáng T2-T4-T6",
                    LevelId = level.Id,
                    TeacherId = teacher.Id,
                    MaxStudents = 30,
                    Status = "ACTIVE",
                    Description = $"Lớp luyện tiếng Anh trình độ {level.Name} vào các buổi sáng Thứ 2, 4, 6.",
                    StartDate = DateTime.UtcNow.AddDays(7),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                morningClass.ClassSchedules.Add(new ClassSchedule { DayOfWeek = 1, StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(9, 30, 0) });
                morningClass.ClassSchedules.Add(new ClassSchedule { DayOfWeek = 3, StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(9, 30, 0) });
                morningClass.ClassSchedules.Add(new ClassSchedule { DayOfWeek = 5, StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(9, 30, 0) });
                classrooms.Add(morningClass);

                // Create Evening Class
                var eveningTeacher = teachers[teacherIdx % teachers.Count];
                teacherIdx++;

                var eveningClass = new Classroom
                {
                    ClassName = $"Lớp {level.Code} - Tối T3-T5-T7",
                    LevelId = level.Id,
                    TeacherId = eveningTeacher.Id,
                    MaxStudents = 25,
                    Status = "ACTIVE",
                    Description = $"Lớp luyện tiếng Anh trình độ {level.Name} vào các buổi tối Thứ 3, 5, 7.",
                    StartDate = DateTime.UtcNow.AddDays(10),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                eveningClass.ClassSchedules.Add(new ClassSchedule { DayOfWeek = 2, StartTime = new TimeSpan(18, 30, 0), EndTime = new TimeSpan(20, 0, 0) });
                eveningClass.ClassSchedules.Add(new ClassSchedule { DayOfWeek = 4, StartTime = new TimeSpan(18, 30, 0), EndTime = new TimeSpan(20, 0, 0) });
                eveningClass.ClassSchedules.Add(new ClassSchedule { DayOfWeek = 6, StartTime = new TimeSpan(18, 30, 0), EndTime = new TimeSpan(20, 0, 0) });
                classrooms.Add(eveningClass);
            }

            _context.Classrooms.AddRange(classrooms);
            await _context.SaveChangesAsync();
        }
    }
}
