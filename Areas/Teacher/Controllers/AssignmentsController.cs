using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Roles = "TEACHER")]
    public class AssignmentsController : Controller
    {
        private readonly ITeacherAssignmentService _assignmentService;

        public AssignmentsController(ITeacherAssignmentService assignmentService)
        {
            _assignmentService = assignmentService;
        }

        // GET: Teacher/Assignments
        public async Task<IActionResult> Index(int? topicId, string? keyword, int page = 1, int pageSize = 10)
        {
            int totalItems = await _assignmentService.GetTotalPracticeTasksAsync(topicId, keyword);
            var items = await _assignmentService.GetPracticeTasksAsync(topicId, keyword, page, pageSize);

            ViewBag.Keyword = keyword;
            ViewBag.TopicId = topicId;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var topics = await _assignmentService.GetActiveTopicsAsync();
            ViewData["Topics"] = new SelectList(topics, "Id", "Title", topicId);

            return View(items);
        }

        // GET: Teacher/Assignments/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var task = await _assignmentService.GetPracticeTaskByIdAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            return View(task);
        }

        // GET: Teacher/Assignments/Create
        public async Task<IActionResult> Create()
        {
            var topics = await _assignmentService.GetActiveTopicsAsync();
            var skills = await _assignmentService.GetActiveSkillsAsync();
            ViewData["TopicId"] = new SelectList(topics, "Id", "Title");
            ViewData["SkillId"] = new SelectList(skills, "Id", "SkillName");
            return View();
        }

        // POST: Teacher/Assignments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Instruction,TopicId,SkillId,DifficultyLevel,TaskType,Status")] PracticeTask task)
        {
            if (ModelState.IsValid)
            {
                task.CreatedAt = DateTime.UtcNow;
                await _assignmentService.CreatePracticeTaskAsync(task);
                TempData["SuccessMessage"] = "Đã tạo bài tập mới thành công!";
                return RedirectToAction(nameof(Index));
            }
            var topics = await _assignmentService.GetActiveTopicsAsync();
            var skills = await _assignmentService.GetActiveSkillsAsync();
            ViewData["TopicId"] = new SelectList(topics, "Id", "Title", task.TopicId);
            ViewData["SkillId"] = new SelectList(skills, "Id", "SkillName", task.SkillId);
            return View(task);
        }

        // GET: Teacher/Assignments/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var task = await _assignmentService.GetPracticeTaskByIdAsync(id);
            if (task == null)
            {
                return NotFound();
            }
            var topics = await _assignmentService.GetActiveTopicsAsync();
            var skills = await _assignmentService.GetActiveSkillsAsync();
            ViewData["TopicId"] = new SelectList(topics, "Id", "Title", task.TopicId);
            ViewData["SkillId"] = new SelectList(skills, "Id", "SkillName", task.SkillId);
            return View(task);
        }

        // POST: Teacher/Assignments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Instruction,TopicId,SkillId,DifficultyLevel,TaskType,Status")] PracticeTask task)
        {
            if (id != task.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingTask = await _assignmentService.GetPracticeTaskByIdAsync(id);
                    if (existingTask == null)
                    {
                        return NotFound();
                    }

                    existingTask.Title = task.Title;
                    existingTask.Instruction = task.Instruction;
                    existingTask.TopicId = task.TopicId;
                    existingTask.SkillId = task.SkillId;
                    existingTask.DifficultyLevel = task.DifficultyLevel;
                    existingTask.TaskType = task.TaskType ?? existingTask.TaskType;
                    existingTask.Status = task.Status ?? existingTask.Status;

                    await _assignmentService.UpdatePracticeTaskAsync(existingTask);
                    TempData["SuccessMessage"] = "Cập nhật bài tập thành công!";
                }
                catch (Exception)
                {
                    if (!await _assignmentService.PracticeTaskExistsAsync(task.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            var topics = await _assignmentService.GetActiveTopicsAsync();
            var skills = await _assignmentService.GetActiveSkillsAsync();
            ViewData["TopicId"] = new SelectList(topics, "Id", "Title", task.TopicId);
            ViewData["SkillId"] = new SelectList(skills, "Id", "SkillName", task.SkillId);
            return View(task);
        }

        // GET: Teacher/Assignments/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var task = await _assignmentService.GetPracticeTaskByIdAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            return View(task);
        }

        // POST: Teacher/Assignments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var task = await _assignmentService.GetPracticeTaskByIdAsync(id);
            if (task != null)
            {
                await _assignmentService.DeletePracticeTaskAsync(task);
                TempData["SuccessMessage"] = "Xóa bài tập thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Teacher/Assignments/Submissions
        public async Task<IActionResult> Submissions(int? taskId, string? status, int page = 1, int pageSize = 12)
        {
            int totalItems = await _assignmentService.GetTotalSubmissionsAsync(taskId, status);
            var items = await _assignmentService.GetSubmissionsAsync(taskId, status, page, pageSize);

            ViewBag.TaskId = taskId;
            ViewBag.Status = status;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var allTasks = await _assignmentService.GetAllPracticeTasksAsync();
            ViewData["Tasks"] = new SelectList(allTasks, "Id", "Title", taskId);

            return View(items);
        }

        // GET: Teacher/Assignments/Grade/5
        public async Task<IActionResult> Grade(int id)
        {
            var submission = await _assignmentService.GetSubmissionByIdAsync(id);
            if (submission == null)
            {
                return NotFound();
            }
            return View(submission);
        }

        // POST: Teacher/Assignments/Grade/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Grade(int id, decimal Score, string TeacherFeedback)
        {
            var submission = await _assignmentService.GetSubmissionByIdAsync(id);
            if (submission == null)
            {
                return NotFound();
            }

            submission.Score = Score;
            submission.TeacherFeedback = TeacherFeedback;
            submission.Status = "GRADED";

            await _assignmentService.UpdateSubmissionAsync(submission);

            TempData["SuccessMessage"] = "Đã chấm điểm bài tập thành công!";
            return RedirectToAction(nameof(Submissions), new { taskId = submission.PracticeTaskId });
        }
    }
}
