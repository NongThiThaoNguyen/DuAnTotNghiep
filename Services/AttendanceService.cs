using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.ViewModels.Teacher;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly ApplicationDbContext _context;

        public AttendanceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<StudentAttendanceViewModel>> GetAttendanceListAsync(int topicId, DateOnly date)
        {
            var studentAttendances = new List<StudentAttendanceViewModel>();

            // Find active students enrolled in this topic
            var students = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.Role.RoleCode == "STUDENT" && u.Status == "ACTIVE" &&
                    _context.StudentLearningPaths.Any(slp =>
                        slp.StudentId == u.Id &&
                        slp.Status == "ACTIVE" &&
                        _context.LearningPathNodes.Any(lpn => lpn.LearningPathId == slp.Id && lpn.TopicId == topicId)
                    ))
                .OrderBy(u => u.FullName)
                .ToListAsync();

            var records = await _context.Attendances
                .Where(a => a.TopicId == topicId && a.AttendanceDate == date)
                .ToDictionaryAsync(a => a.StudentId);

            foreach (var student in students)
            {
                var status = "PRESENT";
                string? remarks = null;

                if (records.TryGetValue(student.Id, out var rec))
                {
                    status = rec.Status;
                    remarks = rec.Remarks;
                }

                studentAttendances.Add(new StudentAttendanceViewModel
                {
                    StudentId = student.Id,
                    StudentName = student.FullName,
                    StudentEmail = student.Email,
                    Status = status,
                    Remarks = remarks
                });
            }

            return studentAttendances;
        }

        public async Task SaveAttendanceAsync(int topicId, DateOnly date, List<StudentAttendanceViewModel> attendances)
        {
            if (attendances == null || !attendances.Any())
                return;

            foreach (var att in attendances)
            {
                var record = await _context.Attendances
                    .FirstOrDefaultAsync(a => a.TopicId == topicId && a.StudentId == att.StudentId && a.AttendanceDate == date);

                if (record == null)
                {
                    record = new Attendance
                    {
                        TopicId = topicId,
                        StudentId = att.StudentId,
                        AttendanceDate = date,
                        Status = att.Status,
                        Remarks = att.Remarks,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.Add(record);
                }
                else
                {
                    record.Status = att.Status;
                    record.Remarks = att.Remarks;
                    record.UpdatedAt = DateTime.UtcNow;
                    _context.Update(record);
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<Attendance>> GetAttendanceHistoryAsync(int? topicId, DateOnly? startDate, DateOnly? endDate)
        {
            var query = _context.Attendances
                .Include(a => a.Student)
                .Include(a => a.Topic)
                .AsNoTracking();

            if (topicId.HasValue)
            {
                query = query.Where(a => a.TopicId == topicId);
            }
            if (startDate.HasValue)
            {
                query = query.Where(a => a.AttendanceDate >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(a => a.AttendanceDate <= endDate.Value);
            }

            return await query
                .OrderByDescending(a => a.AttendanceDate)
                .ThenBy(a => a.Student.FullName)
                .ToListAsync();
        }

        public async Task<List<LearningTopic>> GetActiveTopicsAsync()
        {
            return await _context.LearningTopics
                .Where(t => t.Status == "ACTIVE")
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
