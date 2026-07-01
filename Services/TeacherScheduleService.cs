using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services
{
    public class TeacherScheduleService : ITeacherScheduleService
    {
        private readonly ApplicationDbContext _context;

        public TeacherScheduleService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Schedule>> GetSchedulesAsync(int teacherId, string? keyword, int page, int pageSize)
        {
            var query = _context.Schedules
                .Include(s => s.Topic)
                .Where(s => s.TeacherId == teacherId)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(s => s.Title.Contains(keyword) ||
                                       (s.Description != null && s.Description.Contains(keyword)) ||
                                       (s.Classroom != null && s.Classroom.Contains(keyword)));
            }

            return await query
                .OrderBy(s => s.StartTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalSchedulesCountAsync(int teacherId, string? keyword)
        {
            var query = _context.Schedules
                .Where(s => s.TeacherId == teacherId)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(s => s.Title.Contains(keyword) ||
                                       (s.Description != null && s.Description.Contains(keyword)) ||
                                       (s.Classroom != null && s.Classroom.Contains(keyword)));
            }

            return await query.CountAsync();
        }

        public async Task<Schedule?> GetByIdAsync(int id, int teacherId)
        {
            return await _context.Schedules
                .Include(s => s.Topic)
                .FirstOrDefaultAsync(s => s.Id == id && s.TeacherId == teacherId);
        }

        public async Task CreateAsync(Schedule schedule)
        {
            _context.Add(schedule);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Schedule schedule)
        {
            _context.Update(schedule);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Schedule schedule)
        {
            _context.Schedules.Remove(schedule);
            await _context.SaveChangesAsync();
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
