using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.ViewModels.Student;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services
{
    public class ClassEnrollmentService : IClassEnrollmentService
    {
        private readonly ApplicationDbContext _context;

        public ClassEnrollmentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ClassSelectionViewModel> GetAvailableClassesAsync(int studentId)
        {
            var vm = new ClassSelectionViewModel { StudentId = studentId };

            // 1. Lấy level của Student từ kết quả Placement Test gần nhất
            var latestAttempt = await _context.TestAttempts
                .Include(a => a.EstimatedLevel)
                .Where(a => a.StudentId == studentId
                    && (a.Status == "SUBMITTED" || a.Status == "GRADED")
                    && a.EstimatedLevelId != null)
                .OrderByDescending(a => a.SubmittedAt ?? a.StartedAt)
                .FirstOrDefaultAsync();

            if (latestAttempt?.EstimatedLevel == null)
            {
                // Fallback: lấy từ StudentLearningProfile
                var profile = await _context.StudentLearningProfiles
                    .Include(p => p.CurrentLevel)
                    .FirstOrDefaultAsync(p => p.UserId == studentId);

                if (profile?.CurrentLevel != null)
                {
                    vm.StudentLevel = profile.CurrentLevel.Name;
                    vm.StudentLevelId = profile.CurrentLevel.Id;
                }
                else
                {
                    vm.StudentLevel = "Chưa xác định";
                    vm.StudentLevelId = 0;
                    return vm;
                }
            }
            else
            {
                vm.StudentLevel = latestAttempt.EstimatedLevel.Name;
                vm.StudentLevelId = latestAttempt.EstimatedLevel.Id;
            }

            // 2. Kiểm tra student đã có enrollment active chưa
            var existingEnrollment = await _context.Enrollments
                .Include(e => e.Classroom)
                    .ThenInclude(c => c.Level)
                .Include(e => e.Classroom)
                    .ThenInclude(c => c.Teacher)
                .Include(e => e.Classroom)
                    .ThenInclude(c => c.ClassSchedules)
                .Where(e => e.StudentId == studentId && e.Status == "ACTIVE")
                .FirstOrDefaultAsync();

            if (existingEnrollment != null)
            {
                vm.AlreadyEnrolled = true;
                vm.CurrentEnrollment = new EnrolledClassViewModel
                {
                    EnrollmentId = existingEnrollment.Id,
                    ClassroomId = existingEnrollment.ClassroomId,
                    ClassName = existingEnrollment.Classroom.ClassName,
                    LevelName = existingEnrollment.Classroom.Level.Name,
                    TeacherName = existingEnrollment.Classroom.Teacher.FullName,
                    ScheduleDisplay = FormatSchedule(existingEnrollment.Classroom.ClassSchedules),
                    EnrolledAt = existingEnrollment.EnrolledAt
                };
                return vm;
            }

            // 3. Lấy danh sách lớp phù hợp (cùng level, đang ACTIVE, chưa đầy)
            var classrooms = await _context.Classrooms
                .Include(c => c.Level)
                .Include(c => c.Teacher)
                .Include(c => c.ClassSchedules)
                .Include(c => c.Enrollments)
                .Where(c => c.LevelId == vm.StudentLevelId
                    && c.Status == "ACTIVE")
                .OrderBy(c => c.ClassName)
                .ToListAsync();

            vm.AvailableClasses = classrooms.Select(c =>
            {
                var currentStudents = c.Enrollments.Count(e => e.Status == "ACTIVE");
                return new AvailableClassViewModel
                {
                    ClassroomId = c.Id,
                    ClassName = c.ClassName,
                    LevelName = c.Level.Name,
                    LevelCode = c.Level.Code,
                    TeacherName = c.Teacher.FullName,
                    ScheduleDisplay = FormatSchedule(c.ClassSchedules),
                    CurrentStudents = currentStudents,
                    MaxStudents = c.MaxStudents,
                    StartDate = c.StartDate,
                    Description = c.Description
                };
            }).ToList();

            return vm;
        }

        public async Task<ClassEnrollConfirmViewModel?> GetClassConfirmInfoAsync(int studentId, int classroomId)
        {
            var classroom = await _context.Classrooms
                .Include(c => c.Level)
                .Include(c => c.Teacher)
                .Include(c => c.ClassSchedules)
                .Include(c => c.Enrollments)
                .FirstOrDefaultAsync(c => c.Id == classroomId && c.Status == "ACTIVE");

            if (classroom == null) return null;

            var student = await _context.Users.FirstOrDefaultAsync(u => u.Id == studentId);
            if (student == null) return null;

            // Lấy level của Student
            var latestAttempt = await _context.TestAttempts
                .Include(a => a.EstimatedLevel)
                .Where(a => a.StudentId == studentId
                    && (a.Status == "SUBMITTED" || a.Status == "GRADED")
                    && a.EstimatedLevelId != null)
                .OrderByDescending(a => a.SubmittedAt ?? a.StartedAt)
                .FirstOrDefaultAsync();

            string studentLevel = latestAttempt?.EstimatedLevel?.Name ?? "Chưa xác định";

            var currentStudents = classroom.Enrollments.Count(e => e.Status == "ACTIVE");

            return new ClassEnrollConfirmViewModel
            {
                StudentName = student.FullName,
                StudentLevel = studentLevel,
                ClassInfo = new AvailableClassViewModel
                {
                    ClassroomId = classroom.Id,
                    ClassName = classroom.ClassName,
                    LevelName = classroom.Level.Name,
                    LevelCode = classroom.Level.Code,
                    TeacherName = classroom.Teacher.FullName,
                    ScheduleDisplay = FormatSchedule(classroom.ClassSchedules),
                    CurrentStudents = currentStudents,
                    MaxStudents = classroom.MaxStudents,
                    StartDate = classroom.StartDate,
                    Description = classroom.Description
                }
            };
        }

        public async Task<(bool Success, string? ErrorMessage)> EnrollStudentAsync(int studentId, int classroomId)
        {
            // 1. Kiểm tra student đã hoàn thành Placement Test chưa
            var hasCompleted = await HasCompletedPlacementTestAsync(studentId);
            if (!hasCompleted)
            {
                return (false, "Bạn cần hoàn thành bài kiểm tra đầu vào trước khi chọn lớp.");
            }

            // 2. Kiểm tra student đã có enrollment active chưa (chỉ cho phép 1 lớp)
            var existingEnrollment = await _context.Enrollments
                .AnyAsync(e => e.StudentId == studentId && e.Status == "ACTIVE");
            if (existingEnrollment)
            {
                return (false, "Bạn đã đăng ký lớp học rồi. Mỗi học viên chỉ được đăng ký 1 lớp.");
            }

            // 3. Kiểm tra lớp có tồn tại và đang active
            var classroom = await _context.Classrooms
                .Include(c => c.Enrollments)
                .FirstOrDefaultAsync(c => c.Id == classroomId && c.Status == "ACTIVE");
            if (classroom == null)
            {
                return (false, "Lớp học không tồn tại hoặc đã đóng.");
            }

            // 4. Kiểm tra lớp còn chỗ không
            var currentStudents = classroom.Enrollments.Count(e => e.Status == "ACTIVE");
            if (currentStudents >= classroom.MaxStudents)
            {
                return (false, "Lớp học đã đủ số lượng học viên. Vui lòng chọn lớp khác.");
            }

            // 5. Tạo Enrollment
            var enrollment = new Enrollment
            {
                StudentId = studentId,
                ClassroomId = classroomId,
                Status = "ACTIVE",
                EnrolledAt = DateTime.UtcNow
            };

            _context.Enrollments.Add(enrollment);

            // 6. Cập nhật status lớp nếu đã đầy
            if (currentStudents + 1 >= classroom.MaxStudents)
            {
                classroom.Status = "FULL";
                classroom.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return (true, null);
        }

        public async Task<EnrollmentSuccessViewModel?> GetCurrentEnrollmentAsync(int studentId)
        {
            var enrollment = await _context.Enrollments
                .Include(e => e.Classroom)
                    .ThenInclude(c => c.Level)
                .Include(e => e.Classroom)
                    .ThenInclude(c => c.Teacher)
                .Include(e => e.Classroom)
                    .ThenInclude(c => c.ClassSchedules)
                .Where(e => e.StudentId == studentId && e.Status == "ACTIVE")
                .FirstOrDefaultAsync();

            if (enrollment == null) return null;

            return new EnrollmentSuccessViewModel
            {
                ClassName = enrollment.Classroom.ClassName,
                LevelName = enrollment.Classroom.Level.Name,
                TeacherName = enrollment.Classroom.Teacher.FullName,
                ScheduleDisplay = FormatSchedule(enrollment.Classroom.ClassSchedules),
                EnrolledAt = enrollment.EnrolledAt
            };
        }

        public async Task<bool> HasCompletedPlacementTestAsync(int studentId)
        {
            return await _context.TestAttempts.AnyAsync(a =>
                a.StudentId == studentId &&
                (a.Status == "SUBMITTED" || a.Status == "GRADED"));
        }

        /// <summary>
        /// Format lịch học thành chuỗi hiển thị: "T2, T4: 08:00-09:30"
        /// </summary>
        private string FormatSchedule(ICollection<ClassSchedule> schedules)
        {
            if (schedules == null || !schedules.Any())
                return "Chưa có lịch";

            var dayNames = new Dictionary<int, string>
            {
                { 1, "T2" }, { 2, "T3" }, { 3, "T4" }, { 4, "T5" },
                { 5, "T6" }, { 6, "T7" }, { 7, "CN" }
            };

            // Group by time slot
            var grouped = schedules
                .OrderBy(s => s.DayOfWeek)
                .GroupBy(s => new { s.StartTime, s.EndTime })
                .Select(g =>
                {
                    var days = string.Join(", ", g.Select(s => dayNames.GetValueOrDefault(s.DayOfWeek, $"T{s.DayOfWeek}")));
                    var start = g.Key.StartTime.ToString(@"hh\:mm");
                    var end = g.Key.EndTime.ToString(@"hh\:mm");
                    return $"{days}: {start}-{end}";
                });

            return string.Join(" | ", grouped);
        }
    }
}
