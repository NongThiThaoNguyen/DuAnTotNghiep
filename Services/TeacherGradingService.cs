using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models.ViewModels.Teacher;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Services;

public class TeacherGradingService : ITeacherGradingService
{
    private readonly ApplicationDbContext _context;

    public TeacherGradingService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PendingSubmissionViewModel>> GetPendingSubmissionsAsync(int teacherId)
    {
        return await _context.PracticeSubmissions
            .AsNoTracking()
            .Include(s => s.Student)
            .Include(s => s.PracticeTask).ThenInclude(pt => pt.Topic)
            .Where(s => s.Status == "SUBMITTED" &&
                (s.PracticeTask.CreatedBy == teacherId || s.PracticeTask.Topic!.CreatedBy == teacherId))
            .OrderByDescending(s => s.SubmittedAt)
            .Select(s => new PendingSubmissionViewModel
            {
                Id = s.Id,
                StudentName = s.Student.FullName,
                TaskTitle = s.PracticeTask.Title,
                TopicName = s.PracticeTask.Topic != null ? s.PracticeTask.Topic.Title : "",
                SubmittedAt = s.SubmittedAt
            })
            .ToListAsync();
    }

    public async Task<PracticeSubmissionDetailViewModel?> GetSubmissionDetailAsync(int submissionId)
    {
        return await _context.PracticeSubmissions
            .AsNoTracking()
            .Include(s => s.Student)
            .Include(s => s.PracticeTask)
            .Where(s => s.Id == submissionId)
            .Select(s => new PracticeSubmissionDetailViewModel
            {
                Id = s.Id,
                TopicId = s.PracticeTask.TopicId ?? 0,
                StudentName = s.Student.FullName,
                TaskTitle = s.PracticeTask.Title,
                TaskDescription = s.PracticeTask.Instruction,
                StudentAnswer = s.SubmissionText,
                FileUrl = s.FileUrl,
                AudioUrl = s.AudioUrl,
                Score = s.Score,
                TeacherFeedback = s.TeacherFeedback,
                SubmittedAt = s.SubmittedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task GradeSubmissionAsync(int submissionId, decimal score, string? feedback, int teacherId)
    {
        var submission = await _context.PracticeSubmissions.FindAsync(submissionId);
        if (submission == null)
        {
            return;
        }

        submission.Score = score;
        submission.TeacherFeedback = feedback;
        submission.Status = "GRADED";
        await _context.SaveChangesAsync();
    }

    public async Task<List<GradeOverviewViewModel>> GetGradesOverviewAsync(int? topicId)
    {
        var students = await _context.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .Where(u => u.Role.RoleCode == "STUDENT" && u.Status == "ACTIVE")
            .OrderBy(u => u.FullName)
            .ToListAsync();

        var rows = new List<GradeOverviewViewModel>();

        if (topicId.HasValue)
        {
            var topicName = await _context.LearningTopics.AsNoTracking()
                .Where(t => t.Id == topicId)
                .Select(t => t.Title)
                .FirstOrDefaultAsync() ?? string.Empty;

            foreach (var student in students)
            {
                var quizScores = await _context.QuizAttempts
                    .AsNoTracking()
                    .Include(a => a.Quiz)
                    .Where(a => a.StudentId == student.Id && a.Score.HasValue && a.Quiz.TopicId == topicId)
                    .Select(a => a.Score!.Value)
                    .ToListAsync();

                var practiceScores = await _context.PracticeSubmissions
                    .AsNoTracking()
                    .Include(s => s.PracticeTask)
                    .Where(s => s.StudentId == student.Id && s.Score.HasValue && s.PracticeTask.TopicId == topicId)
                    .Select(s => s.Score!.Value)
                    .ToListAsync();

                if (!quizScores.Any() && !practiceScores.Any())
                {
                    continue;
                }

                var quizScore = quizScores.Any() ? quizScores.Average() : 0;
                var practiceScore = practiceScores.Any() ? practiceScores.Average() : 0;
                var total = quizScores.Any() && practiceScores.Any()
                    ? (quizScore + practiceScore) / 2
                    : quizScores.Any() ? quizScore : practiceScore;

                rows.Add(new GradeOverviewViewModel
                {
                    StudentId = student.Id,
                    StudentName = student.FullName,
                    TopicName = topicName,
                    QuizScore = quizScore,
                    PracticeScore = practiceScore,
                    TotalScore = total
                });
            }
        }
        else
        {
            foreach (var student in students)
            {
                var quizTopicMap = await _context.QuizAttempts
                    .AsNoTracking()
                    .Include(a => a.Quiz).ThenInclude(q => q.Topic)
                    .Where(a => a.StudentId == student.Id && a.Score.HasValue)
                    .GroupBy(a => new { TopicId = a.Quiz.TopicId, TopicTitle = a.Quiz.Topic != null ? a.Quiz.Topic.Title : "Khóa học" })
                    .Select(g => new { g.Key.TopicId, g.Key.TopicTitle, Scores = g.Select(x => x.Score!.Value).ToList() })
                    .ToListAsync();

                var practiceTopicMap = await _context.PracticeSubmissions
                    .AsNoTracking()
                    .Include(s => s.PracticeTask).ThenInclude(pt => pt.Topic)
                    .Where(s => s.StudentId == student.Id && s.Score.HasValue)
                    .GroupBy(s => new { TopicId = s.PracticeTask.TopicId, TopicTitle = s.PracticeTask.Topic != null ? s.PracticeTask.Topic.Title : "Khóa học" })
                    .Select(g => new { g.Key.TopicId, g.Key.TopicTitle, Scores = g.Select(x => x.Score!.Value).ToList() })
                    .ToListAsync();

                var topicKeys = quizTopicMap.Select(q => new { q.TopicId, q.TopicTitle })
                    .Union(practiceTopicMap.Select(p => new { p.TopicId, p.TopicTitle }))
                    .Distinct()
                    .ToList();

                if (!topicKeys.Any())
                {
                    continue;
                }

                foreach (var tk in topicKeys)
                {
                    var quizScores = quizTopicMap.FirstOrDefault(q => q.TopicId == tk.TopicId)?.Scores ?? new List<decimal>();
                    var practiceScores = practiceTopicMap.FirstOrDefault(p => p.TopicId == tk.TopicId)?.Scores ?? new List<decimal>();

                    var quizScore = quizScores.Any() ? quizScores.Average() : 0;
                    var practiceScore = practiceScores.Any() ? practiceScores.Average() : 0;
                    var total = quizScores.Any() && practiceScores.Any()
                        ? (quizScore + practiceScore) / 2
                        : quizScores.Any() ? quizScore : practiceScore;

                    rows.Add(new GradeOverviewViewModel
                    {
                        StudentId = student.Id,
                        StudentName = student.FullName,
                        TopicName = string.IsNullOrWhiteSpace(tk.TopicTitle) ? "Khóa học" : tk.TopicTitle,
                        QuizScore = quizScore,
                        PracticeScore = practiceScore,
                        TotalScore = total
                    });
                }
            }
        }

        return rows;
    }
}
