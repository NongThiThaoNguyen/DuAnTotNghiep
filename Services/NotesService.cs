using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.ViewModels.Student;
using DuAnTotNghiep.Services.Interfaces;

namespace DuAnTotNghiep.Services
{
    public class NotesService : INotesService
    {
        private readonly ApplicationDbContext _context;

        public NotesService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<NoteViewModel>> GetNotesAsync(int userId)
        {
            return await _context.StudentNotes
                .Include(n => n.Topic)
                .Include(n => n.Lesson)
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NoteViewModel
                {
                    Id = n.Id,
                    Title = n.Title,
                    Content = n.Content,
                    CourseName = n.Topic != null ? n.Topic.Title : null,
                    LessonName = n.Lesson != null ? n.Lesson.Title : null,
                    CreatedAt = n.CreatedAt,
                    UpdatedAt = n.UpdatedAt
                })
                .ToListAsync();
        }

        public async Task<NoteViewModel?> GetNoteByIdAsync(int id, int userId)
        {
            var n = await _context.StudentNotes
                .Include(note => note.Topic)
                .Include(note => note.Lesson)
                .FirstOrDefaultAsync(note => note.Id == id && note.UserId == userId);

            if (n == null) return null;

            return new NoteViewModel
            {
                Id = n.Id,
                Title = n.Title,
                Content = n.Content,
                CourseName = n.Topic?.Title,
                LessonName = n.Lesson?.Title,
                CreatedAt = n.CreatedAt,
                UpdatedAt = n.UpdatedAt
            };
        }

        public async Task<bool> CreateNoteAsync(int userId, NoteCreateUpdateViewModel model)
        {
            var note = new StudentNote
            {
                UserId = userId,
                Title = model.Title,
                Content = model.Content,
                TopicId = model.TopicId,
                LessonId = model.LessonId,
                CreatedAt = DateTime.UtcNow
            };

            await _context.StudentNotes.AddAsync(note);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> UpdateNoteAsync(int userId, NoteCreateUpdateViewModel model)
        {
            if (!model.Id.HasValue) return false;

            var note = await _context.StudentNotes.FirstOrDefaultAsync(n => n.Id == model.Id && n.UserId == userId);
            if (note == null) return false;

            note.Title = model.Title;
            note.Content = model.Content;
            note.TopicId = model.TopicId;
            note.LessonId = model.LessonId;
            note.UpdatedAt = DateTime.UtcNow;

            _context.StudentNotes.Update(note);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> DeleteNoteAsync(int id, int userId)
        {
            var note = await _context.StudentNotes.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
            if (note == null) return false;

            _context.StudentNotes.Remove(note);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }
    }
}
