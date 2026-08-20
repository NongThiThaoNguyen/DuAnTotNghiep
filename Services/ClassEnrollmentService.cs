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

            // 2. Kiểm tra student đã có enrollment active hoặc pending teacher chưa
            var existingEnrollment = await _context.Enrollments
                .Include(e => e.Classroom)
                    .ThenInclude(c => c.Level)
                .Include(e => e.Classroom)
                    .ThenInclude(c => c.Teacher)
                .Include(e => e.Classroom)
                    .ThenInclude(c => c.ClassSchedules)
                .Where(e => e.StudentId == studentId && (e.Status == "ACTIVE" || e.Status == "PENDING_TEACHER"))
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
                    TeacherName = existingEnrollment.Classroom.Teacher?.FullName ?? "Chưa gán",
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
                var currentStudents = c.Enrollments.Count(e => e.Status == "ACTIVE" || e.Status == "PENDING_TEACHER");
                return new AvailableClassViewModel
                {
                    ClassroomId = c.Id,
                    ClassName = c.ClassName,
                    LevelName = c.Level.Name,
                    LevelCode = c.Level.Code,
                    TeacherName = c.Teacher?.FullName ?? "Chưa chọn giáo viên",
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

            var currentStudents = classroom.Enrollments.Count(e => e.Status == "ACTIVE" || e.Status == "PENDING_TEACHER");

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
                    TeacherName = classroom.Teacher?.FullName ?? "Chưa chọn giáo viên",
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
                return (false, "Bạn cần hoàn thành bài kiểm tra đầu vào và được đánh giá trình độ trước khi chọn lớp.");
            }

            // 2. Lấy level của student
            var latestAttempt = await _context.TestAttempts
                .Where(a => a.StudentId == studentId && (a.Status == "SUBMITTED" || a.Status == "GRADED") && a.EstimatedLevelId != null)
                .OrderByDescending(a => a.SubmittedAt ?? a.StartedAt)
                .FirstOrDefaultAsync();

            int? studentLevelId = latestAttempt?.EstimatedLevelId;
            if (!studentLevelId.HasValue)
            {
                var profile = await _context.StudentLearningProfiles
                    .FirstOrDefaultAsync(p => p.UserId == studentId);
                studentLevelId = profile?.CurrentLevelId;
            }

            // 3. Kiểm tra student đã chọn lớp chưa (chỉ cho phép 1 lớp)
            var existingEnrollment = await _context.Enrollments
                .AnyAsync(e => e.StudentId == studentId && (e.Status == "ACTIVE" || e.Status == "PENDING_TEACHER"));
            if (existingEnrollment)
            {
                return (false, "Bạn đã đăng ký chọn lớp học rồi. Mỗi học viên chỉ được chọn 1 lớp.");
            }

            // 4. Kiểm tra lớp có tồn tại và đang active
            var classroom = await _context.Classrooms
                .Include(c => c.Level)
                .Include(c => c.Enrollments)
                .FirstOrDefaultAsync(c => c.Id == classroomId && c.Status == "ACTIVE");
            if (classroom == null)
            {
                return (false, "Lớp học không tồn tại hoặc đã đóng.");
            }

            // 5. Kiểm tra trình độ lớp có phù hợp với trình độ của student không
            if (studentLevelId.HasValue && classroom.LevelId != studentLevelId.Value)
            {
                return (false, "Lớp học này không phù hợp với trình độ đã được đánh giá của bạn.");
            }

            // 6. Kiểm tra lớp còn chỗ không
            var currentStudents = classroom.Enrollments.Count(e => e.Status == "ACTIVE" || e.Status == "PENDING_TEACHER");
            if (currentStudents >= classroom.MaxStudents)
            {
                return (false, "Lớp học đã đủ số lượng học viên. Vui lòng chọn lớp khác.");
            }

            // 7. Tạo Enrollment với trạng thái PENDING_TEACHER (Lưu lựa chọn lớp, chưa gán giáo viên/đưa vào danh sách chính thức)
            var enrollment = new Enrollment
            {
                StudentId = studentId,
                ClassroomId = classroomId,
                Status = "PENDING_TEACHER",
                EnrolledAt = DateTime.UtcNow
            };

            _context.Enrollments.Add(enrollment);

            // 8. Cập nhật status lớp nếu đã đầy
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
                .Where(e => e.StudentId == studentId && (e.Status == "ACTIVE" || e.Status == "PENDING_TEACHER"))
                .FirstOrDefaultAsync();

            if (enrollment == null) return null;

            return new EnrollmentSuccessViewModel
            {
                ClassName = enrollment.Classroom.ClassName,
                LevelName = enrollment.Classroom.Level.Name,
                TeacherName = enrollment.Classroom.Teacher?.FullName ?? "Chưa gán giáo viên",
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

        // ─────────────────────────────────────────────────────────────
        // PHẦN 4: CHỌN GIÁO VIÊN — Implementations
        // ─────────────────────────────────────────────────────────────

        public async Task<TeacherSelectionViewModel?> GetTeacherSelectionAsync(int studentId)
        {
            // Lấy enrollment đang ở PENDING_TEACHER
            var enrollment = await _context.Enrollments
                .Include(e => e.Classroom)
                    .ThenInclude(c => c.Level)
                .Include(e => e.Classroom)
                    .ThenInclude(c => c.ClassSchedules)
                .Include(e => e.Student)
                .FirstOrDefaultAsync(e => e.StudentId == studentId && e.Status == "PENDING_TEACHER");

            if (enrollment == null) return null;

            int levelId = enrollment.Classroom.LevelId;

            // Lấy tất cả lớp cùng level đang ACTIVE (bao gồm cả lớp hiện tại)
            var classrooms = await _context.Classrooms
                .Include(c => c.Teacher)
                    .ThenInclude(t => t.UserProfile)
                .Include(c => c.ClassSchedules)
                .Include(c => c.Enrollments)
                .Where(c => c.LevelId == levelId && c.Status == "ACTIVE")
                .ToListAsync();

            var teachers = classrooms.Select(c =>
            {
                var currentStudents = c.Enrollments.Count(e =>
                    e.Status == "ACTIVE" || e.Status == "PENDING_TEACHER" || e.Status == "AWAITING_CONFIRMATION");
                var bio = c.Teacher?.UserProfile?.Bio;

                return new TeacherOptionViewModel
                {
                    TeacherId = c.TeacherId,
                    ClassroomId = c.Id,
                    TeacherName = c.Teacher?.FullName ?? "Chưa có tên",
                    AvatarUrl = c.Teacher?.AvatarUrl,
                    Bio = bio,
                    ClassName = c.ClassName,
                    ScheduleDisplay = FormatSchedule(c.ClassSchedules),
                    CurrentStudents = currentStudents,
                    MaxStudents = c.MaxStudents
                };
            }).ToList();

            return new TeacherSelectionViewModel
            {
                StudentId = studentId,
                StudentName = enrollment.Student?.FullName ?? "",
                ClassName = enrollment.Classroom.ClassName,
                LevelName = enrollment.Classroom.Level.Name,
                CurrentClassroomId = enrollment.ClassroomId,
                Teachers = teachers
            };
        }

        public async Task<(bool Success, string? ErrorMessage)> SelectTeacherAsync(int studentId, int classroomId)
        {
            // Lấy enrollment PENDING_TEACHER của student
            var enrollment = await _context.Enrollments
                .Include(e => e.Classroom)
                .FirstOrDefaultAsync(e => e.StudentId == studentId && e.Status == "PENDING_TEACHER");

            if (enrollment == null)
                return (false, "Không tìm thấy thông tin đăng ký lớp. Vui lòng chọn lớp trước.");

            // Kiểm tra lớp mới có tồn tại và cùng level
            var targetClassroom = await _context.Classrooms
                .Include(c => c.Enrollments)
                .FirstOrDefaultAsync(c => c.Id == classroomId && c.Status == "ACTIVE");

            if (targetClassroom == null)
                return (false, "Lớp học của giáo viên này không khả dụng.");

            if (targetClassroom.LevelId != enrollment.Classroom.LevelId)
                return (false, "Giáo viên này không phù hợp với trình độ của bạn.");

            var currentStudents = targetClassroom.Enrollments.Count(e =>
                e.Status == "ACTIVE" || e.Status == "PENDING_TEACHER" || e.Status == "AWAITING_CONFIRMATION");

            if (currentStudents >= targetClassroom.MaxStudents)
                return (false, "Lớp của giáo viên này đã đủ số lượng học sinh.");

            // Nếu chuyển sang lớp khác, kiểm tra lớp cũ có cần cập nhật không
            if (enrollment.ClassroomId != classroomId)
            {
                var oldClassroom = await _context.Classrooms.FindAsync(enrollment.ClassroomId);
                if (oldClassroom != null && oldClassroom.Status == "FULL")
                {
                    oldClassroom.Status = "ACTIVE";
                    oldClassroom.UpdatedAt = DateTime.UtcNow;
                }
            }

            // Cập nhật enrollment
            enrollment.ClassroomId = classroomId;
            enrollment.Status = "AWAITING_CONFIRMATION";

            await _context.SaveChangesAsync();

            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> SelectRandomTeacherAsync(int studentId)
        {
            var vm = await GetTeacherSelectionAsync(studentId);
            if (vm == null)
                return (false, "Không tìm thấy thông tin đăng ký lớp. Vui lòng chọn lớp trước.");

            var available = vm.Teachers.Where(t => !t.IsFull).ToList();
            if (!available.Any())
                return (false, "Hiện không có giáo viên nào còn chỗ trống. Vui lòng thử lại sau.");

            var rng = new Random();
            var picked = available[rng.Next(available.Count)];

            return await SelectTeacherAsync(studentId, picked.ClassroomId);
        }

        public async Task<TeacherSelectedSuccessViewModel?> GetTeacherSelectedSuccessAsync(int studentId)
        {
            var enrollment = await _context.Enrollments
                .Include(e => e.Classroom)
                    .ThenInclude(c => c.Level)
                .Include(e => e.Classroom)
                    .ThenInclude(c => c.Teacher)
                .Include(e => e.Classroom)
                    .ThenInclude(c => c.ClassSchedules)
                .Include(e => e.Student)
                .FirstOrDefaultAsync(e => e.StudentId == studentId && e.Status == "AWAITING_CONFIRMATION");

            if (enrollment == null) return null;

            return new TeacherSelectedSuccessViewModel
            {
                StudentName = enrollment.Student?.FullName ?? "",
                TeacherName = enrollment.Classroom.Teacher?.FullName ?? "Chưa xác định",
                ClassName = enrollment.Classroom.ClassName,
                LevelName = enrollment.Classroom.Level.Name,
                ScheduleDisplay = FormatSchedule(enrollment.Classroom.ClassSchedules),
                Status = "Chờ xác nhận đăng ký"
            };
        }

        // ─────────────────────────────────────────────────────────────
        // PHẦN 5: XÁC NHẬN PHÂN LỚP VÀ CẬP NHẬT DANH SÁCH — Implementations
        // ─────────────────────────────────────────────────────────────

        public async Task<(bool Success, string? ErrorMessage)> ConfirmFinalEnrollmentAsync(int studentId)
        {
            // 1. Kiểm tra enrollment đang ở trạng thái AWAITING_CONFIRMATION hoặc PENDING_TEACHER
            var pendingEnrollments = await _context.Enrollments
                .Include(e => e.Classroom)
                .Where(e => e.StudentId == studentId && (e.Status == "AWAITING_CONFIRMATION" || e.Status == "PENDING_TEACHER"))
                .ToListAsync();

            var activeEnrollments = await _context.Enrollments
                .Where(e => e.StudentId == studentId && e.Status == "ACTIVE")
                .ToListAsync();

            if (!pendingEnrollments.Any())
            {
                if (activeEnrollments.Any())
                {
                    // Học sinh đã có enrollment ACTIVE chính thức
                    return (true, null);
                }
                return (false, "Không tìm thấy thông tin đăng ký chờ xác nhận. Vui lòng thực hiện chọn lớp và giáo viên trước.");
            }

            // Đảm bảo chỉ giữ 1 enrollment duy nhất được confirm
            var targetEnrollment = pendingEnrollments.First();

            // 2. Chuyển trạng thái Enrollment → ACTIVE (Đã chính thức phân lớp & giáo viên)
            targetEnrollment.Status = "ACTIVE";
            targetEnrollment.EnrolledAt = DateTime.UtcNow;

            // Xóa/hủy các bản ghi enrollment chờ khác (tránh trùng lặp hoặc thuộc nhiều lớp)
            foreach (var other in pendingEnrollments.Skip(1))
            {
                _context.Enrollments.Remove(other);
            }
            foreach (var otherActive in activeEnrollments)
            {
                if (otherActive.Id != targetEnrollment.Id)
                {
                    otherActive.Status = "DROPPED";
                }
            }

            // 3. Cập nhật trạng thái Student trong bảng User → "STUDYING" (Đang học)
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == studentId);
            if (user != null)
            {
                user.Status = "STUDYING";
                user.UpdatedAt = DateTime.UtcNow;
            }

            // 4. Cập nhật StudentLearningProfile.OnboardingStatus → "COMPLETED"
            var profile = await _context.StudentLearningProfiles.FirstOrDefaultAsync(p => p.UserId == studentId);
            if (profile != null)
            {
                profile.OnboardingStatus = "COMPLETED";
                profile.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<TeacherSelectedSuccessViewModel?> GetFinalSuccessInfoAsync(int studentId)
        {
            var enrollment = await _context.Enrollments
                .Include(e => e.Classroom)
                    .ThenInclude(c => c.Level)
                .Include(e => e.Classroom)
                    .ThenInclude(c => c.Teacher)
                .Include(e => e.Classroom)
                    .ThenInclude(c => c.ClassSchedules)
                .Include(e => e.Student)
                .Where(e => e.StudentId == studentId && e.Status == "ACTIVE")
                .OrderByDescending(e => e.EnrolledAt)
                .FirstOrDefaultAsync();

            if (enrollment == null) return null;

            return new TeacherSelectedSuccessViewModel
            {
                StudentName = enrollment.Student?.FullName ?? "",
                TeacherName = enrollment.Classroom.Teacher?.FullName ?? "Chưa xác định",
                ClassName = enrollment.Classroom.ClassName,
                LevelName = enrollment.Classroom.Level.Name,
                ScheduleDisplay = FormatSchedule(enrollment.Classroom.ClassSchedules),
                Status = "Đang học"
            };
        }
    }
}
