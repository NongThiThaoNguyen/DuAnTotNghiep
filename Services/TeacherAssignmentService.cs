using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services
{
    public class TeacherAssignmentService : ITeacherAssignmentService
    {
        private readonly ApplicationDbContext _context;

        public TeacherAssignmentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetTotalPracticeTasksAsync(int? topicId, string? keyword)
        {
            var query = _context.PracticeTasks.AsNoTracking();
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(t => t.Title.Contains(keyword) || t.Instruction.Contains(keyword));
            }
            if (topicId.HasValue)
            {
                query = query.Where(t => t.TopicId == topicId);
            }
            return await query.CountAsync();
        }

        public async Task<List<PracticeTask>> GetPracticeTasksAsync(int? topicId, string? keyword, int page, int pageSize)
        {
            var query = _context.PracticeTasks
                .Include(t => t.Topic)
                .Include(t => t.Skill)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(t => t.Title.Contains(keyword) || t.Instruction.Contains(keyword));
            }
            if (topicId.HasValue)
            {
                query = query.Where(t => t.TopicId == topicId);
            }

            return await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<PracticeTask?> GetPracticeTaskByIdAsync(int id)
        {
            return await _context.PracticeTasks
                .Include(t => t.Topic)
                .Include(t => t.Skill)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task CreatePracticeTaskAsync(PracticeTask task)
        {
            _context.PracticeTasks.Add(task);
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePracticeTaskAsync(PracticeTask task)
        {
            _context.PracticeTasks.Update(task);
            await _context.SaveChangesAsync();
        }

        public async Task DeletePracticeTaskAsync(PracticeTask task)
        {
            _context.PracticeTasks.Remove(task);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> PracticeTaskExistsAsync(int id)
        {
            return await _context.PracticeTasks.AnyAsync(e => e.Id == id);
        }

        public async Task<int> GetTotalSubmissionsAsync(int? taskId, string? status)
        {
            var query = _context.PracticeSubmissions.AsNoTracking();
            if (taskId.HasValue)
            {
                query = query.Where(s => s.PracticeTaskId == taskId);
            }
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(s => s.Status == status);
            }
            return await query.CountAsync();
        }

        public async Task<List<PracticeSubmission>> GetSubmissionsAsync(int? taskId, string? status, int page, int pageSize)
        {
            var query = _context.PracticeSubmissions
                .Include(s => s.Student)
                .Include(s => s.PracticeTask)
                .AsNoTracking();

            if (taskId.HasValue)
            {
                query = query.Where(s => s.PracticeTaskId == taskId);
            }
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(s => s.Status == status);
            }

            return await query
                .OrderByDescending(s => s.SubmittedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<PracticeSubmission?> GetSubmissionByIdAsync(int id)
        {
            return await _context.PracticeSubmissions
                .Include(s => s.Student)
                .Include(s => s.PracticeTask)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task UpdateSubmissionAsync(PracticeSubmission submission)
        {
            _context.PracticeSubmissions.Update(submission);
            await _context.SaveChangesAsync();
        }

        public async Task<List<LearningTopic>> GetActiveTopicsAsync()
        {
            return await _context.LearningTopics
                .Where(t => t.Status == "ACTIVE")
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<EnglishSkill>> GetActiveSkillsAsync()
        {
            return await _context.EnglishSkills
                .Where(s => s.IsActive)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<PracticeTask>> GetAllPracticeTasksAsync()
        {
            return await _context.PracticeTasks.AsNoTracking().ToListAsync();
        }
    }
}
