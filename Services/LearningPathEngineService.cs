using DuAnTotNghiep.Data;
using DuAnTotNghiep.DTOs.LearningPath;
using DuAnTotNghiep.Enums;
using DuAnTotNghiep.Exceptions;
using DuAnTotNghiep.Helpers;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Repositories.Interfaces;
using DuAnTotNghiep.Services.Interfaces;
using DuAnTotNghiep.ViewModels.LearningPath.M8;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Services;

/// <summary>
/// Builds AI learning path inputs and coordinates path generation lifecycle operations.
/// </summary>
public class LearningPathEngineService : ILearningPathEngineService
{
    private readonly ApplicationDbContext _context;
    private readonly ILearningPathRepository _learningPathRepository;
    private readonly ILearningPathAiService _learningPathAiService;
    private readonly ILearningPathComplianceService _learningPathComplianceService;
    private readonly ILogger<LearningPathEngineService> _logger;

    public LearningPathEngineService(
        ApplicationDbContext context,
        ILearningPathRepository learningPathRepository,
        ILogger<LearningPathEngineService> logger)
        : this(
            context,
            learningPathRepository,
            logger,
            new LearningPathAiService(),
            new LearningPathComplianceService(context))
    {
    }

    public LearningPathEngineService(
        ApplicationDbContext context,
        ILearningPathRepository learningPathRepository,
        ILogger<LearningPathEngineService> logger,
        ILearningPathAiService learningPathAiService)
        : this(
            context,
            learningPathRepository,
            logger,
            learningPathAiService,
            new LearningPathComplianceService(context))
    {
    }

    public LearningPathEngineService(
        ApplicationDbContext context,
        ILearningPathRepository learningPathRepository,
        ILogger<LearningPathEngineService> logger,
        ILearningPathAiService learningPathAiService,
        ILearningPathComplianceService learningPathComplianceService)
    {
        _context = context;
        _learningPathRepository = learningPathRepository;
        _learningPathAiService = learningPathAiService;
        _learningPathComplianceService = learningPathComplianceService;
        _logger = logger;
    }

    public Task<LearningPathInputDto> BuildInputAsync(int studentId) =>
        LearningPathInputBuilder.BuildInputAsync(_context, studentId);

    public async Task<LearningPathGenerateViewModel> CanGeneratePathAsync(int studentId)
    {
        return await LearningPathInputBuilder.CanGeneratePathAsync(_context, _learningPathRepository, studentId);
    }

    public Task<StudentLearningPath> GenerateInitialPathAsync(int studentId, int competencyAnalysisId) =>
        GenerateInitialPathCoreAsync(studentId, competencyAnalysisId);

    public async Task<StudentLearningPath?> GetActivePathAsync(int studentId)
    {
        return await _learningPathRepository.GetActivePathByStudentIdAsync(studentId);
    }

    public async Task<LearningPathDetailViewModel> GetPathDetailAsync(int pathId, int userId)
    {
        var path = await GetOwnedPathWithNodesAsync(pathId, userId);
        return LearningPathViewModelFactory.CreateDetail(path);
    }

    public async Task<LearningPathSummaryViewModel> GetPathSummaryAsync(int studentId)
    {
        var activePath = await GetRequiredActivePathAsync(studentId);
        var path = await GetOwnedPathWithNodesAsync(activePath.Id, studentId);
        return LearningPathViewModelFactory.CreateSummary(path);
    }

    public async Task ArchivePathAsync(int pathId, int userId)
    {
        var path = await GetOwnedTrackedPathAsync(pathId, userId);
        ArchivePath(path);
        await _context.SaveChangesAsync();
    }

    public async Task<StudentLearningPath> RegeneratePathAsync(int studentId, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason)) throw new BusinessException("Lý do tạo lại lộ trình là bắt buộc.");

        await LearningPathRegenerationHelper.EnsureDailyLimitAsync(_context, studentId);
        var oldPath = await GetRequiredActivePathAsync(studentId);
        ArchivePath(oldPath);
        var newPath = await GenerateReplacementPathAsync(studentId, oldPath);
        oldPath.ReplacedByPathId = newPath.Id;
        LearningPathRegenerationHelper.AddReplanningEvent(_context, studentId, oldPath, newPath, reason);
        await _learningPathRepository.UpdatePathAsync(oldPath);
        await _context.SaveChangesAsync();
        return newPath;
    }

    public async Task<bool> UnlockNextNodeAsync(int completedNodeId, int studentId)
    {
        var completedNode = await _context.LearningPathNodes
            .Include(node => node.LearningPath)
            .FirstOrDefaultAsync(node => node.Id == completedNodeId);
        if (completedNode == null) throw new NotFoundException("Không tìm thấy node học tập.");
        if (completedNode.LearningPath.StudentId != studentId) throw new UnauthorizedAccessException("Bạn không có quyền mở node này.");
        if (completedNode.Status != ProgressStatus.Completed) return false;

        var nextNode = await GetNextNodeAsync(completedNode);
        if (nextNode == null || nextNode.Status != ProgressStatus.Locked) return false;
        if (!await IsPrerequisiteSatisfiedAsync(nextNode)) return false;

        nextNode.Status = ProgressStatus.Available;
        await _context.SaveChangesAsync();
        return true;
    }

    private async Task<StudentLearningPath> GenerateInitialPathCoreAsync(int studentId, int competencyAnalysisId)
    {
        EnsureCanGenerate(await CanGeneratePathAsync(studentId));
        var input = await BuildInputAsync(studentId);
        var aiResult = await LearningPathAiGenerationHelper.TryGenerateAsync(
            _context,
            _learningPathAiService,
            input,
            studentId);
        if (!aiResult.IsSuccess)
        {
            return await SaveFallbackOrFailedPathAsync(studentId, competencyAnalysisId, aiResult.ErrorMessage);
        }

        var output = aiResult.Output!;
        var mapped = await MapAiOutputToPathAsync(output, studentId, competencyAnalysisId);
        await SaveGeneratedPathAsync(mapped.Path, mapped.Nodes);
        _logger.LogInformation("Generated learning path {PathId} for student {StudentId}", mapped.Path.Id, studentId);
        return mapped.Path;
    }

    private async Task<StudentLearningPath> GenerateReplacementPathAsync(
        int studentId,
        StudentLearningPath oldPath)
    {
        var input = await BuildInputAsync(studentId);
        var output = LearningPathMockOutputBuilder.Generate(input);
        await LearningPathReferenceValidator.ValidateAsync(_context, output);
        var analysisId = oldPath.CompetencyAnalysisId ?? (await LearningPathInputBuilder.GetLatestAnalysisAsync(_context, studentId)).Id;
        var mapped = await MapAiOutputToPathAsync(output, studentId, analysisId);
        mapped.Path.PathVersion = oldPath.PathVersion + 1;
        await SaveGeneratedPathAsync(mapped.Path, mapped.Nodes);
        return mapped.Path;
    }

    private async Task<StudentLearningPath> SaveFallbackOrFailedPathAsync(
        int studentId,
        int competencyAnalysisId,
        string errorMessage)
    {
        var profile = await LearningPathInputBuilder.GetProfileAsync(_context, studentId);
        var template = await LearningPathFallbackFactory.GetMatchingTemplateAsync(_context, profile);
        if (template != null)
        {
            var mapped = LearningPathFallbackFactory.CreateTemplatePath(
                template,
                profile,
                studentId,
                competencyAnalysisId,
                errorMessage,
                DateTime.UtcNow);
            await SaveGeneratedPathAsync(mapped.Path, mapped.Nodes);
            return mapped.Path;
        }

        var failedPath = LearningPathFallbackFactory.CreateFailedPath(
            profile,
            studentId,
            competencyAnalysisId,
            errorMessage,
            DateTime.UtcNow);
        await SaveGeneratedPathAsync(failedPath, new List<LearningPathNode>());
        return failedPath;
    }

    private static void EnsureCanGenerate(LearningPathGenerateViewModel readiness)
    {
        if (!string.IsNullOrWhiteSpace(readiness.MissingStep))
        {
            throw new BusinessException(readiness.MissingStep);
        }
    }

    private async Task<StudentLearningPath> GetRequiredActivePathAsync(int studentId)
    {
        var path = await _context.StudentLearningPaths
            .FirstOrDefaultAsync(p => p.StudentId == studentId && p.Status == LearningPathStatus.Active);

        return path ?? throw new NotFoundException("Không tìm thấy lộ trình học đang hoạt động.");
    }

    private async Task<StudentLearningPath> GetOwnedPathWithNodesAsync(int pathId, int userId)
    {
        var path = await _learningPathRepository.GetPathWithNodesAsync(pathId);
        if (path == null) throw new NotFoundException("Không tìm thấy lộ trình học.");
        if (path.StudentId != userId) throw new UnauthorizedAccessException("Bạn không có quyền truy cập lộ trình này.");
        return path;
    }

    private async Task<StudentLearningPath> GetOwnedTrackedPathAsync(int pathId, int userId)
    {
        var path = await _context.StudentLearningPaths.FirstOrDefaultAsync(p => p.Id == pathId);
        if (path == null) throw new NotFoundException("Không tìm thấy lộ trình học.");
        if (path.StudentId != userId) throw new UnauthorizedAccessException("Bạn không có quyền truy cập lộ trình này.");
        return path;
    }

    private static void ArchivePath(StudentLearningPath path)
    {
        path.Status = LearningPathStatus.Archived;
        path.ArchivedAt = DateTime.UtcNow;
        path.UpdatedAt = DateTime.UtcNow;
    }

    private async Task<LearningPathNode?> GetNextNodeAsync(LearningPathNode completedNode)
    {
        return await _context.LearningPathNodes.FirstOrDefaultAsync(node =>
            node.LearningPathId == completedNode.LearningPathId
            && node.OrderIndex == completedNode.OrderIndex + 1);
    }

    private async Task<bool> IsPrerequisiteSatisfiedAsync(LearningPathNode node)
    {
        if (!node.RequiredNodeId.HasValue) return true;
        return await _context.LearningPathNodes.AnyAsync(prerequisite =>
            prerequisite.Id == node.RequiredNodeId.Value
            && prerequisite.LearningPathId == node.LearningPathId
            && prerequisite.Status == ProgressStatus.Completed);
    }

    private async Task<(StudentLearningPath Path, List<LearningPathNode> Nodes)> MapAiOutputToPathAsync(
        LearningPathOutputDto output,
        int studentId,
        int competencyAnalysisId)
    {
        var profile = await LearningPathInputBuilder.GetProfileAsync(_context, studentId);
        var now = DateTime.UtcNow;
        var path = LearningPathEntityFactory.CreatePath(output, profile, studentId, competencyAnalysisId, now);
        var nodes = output.Phases
            .SelectMany(p => p.Nodes)
            .Select((n, index) => LearningPathEntityFactory.CreateNode(n, path, index + 1, now))
            .ToList();

        return (path, nodes);
    }

    private async Task SaveGeneratedPathAsync(StudentLearningPath path, List<LearningPathNode> nodes)
    {
        await EnsureContentComplianceAsync(nodes);
        if (_context.Database.IsRelational())
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            await AddPathAndNodesAsync(path, nodes);
            await transaction.CommitAsync();
            return;
        }

        await AddPathAndNodesAsync(path, nodes);
    }

    private async Task EnsureContentComplianceAsync(List<LearningPathNode> nodes)
    {
        var compliance = await _learningPathComplianceService.ValidateContentComplianceAsync(nodes);
        if (!compliance.IsCompliant) throw new BusinessException(string.Join("; ", compliance.Violations));
    }

    private async Task AddPathAndNodesAsync(StudentLearningPath path, List<LearningPathNode> nodes)
    {
        await _learningPathRepository.AddPathAsync(path);
        await _learningPathRepository.AddNodesAsync(nodes);
        await _context.SaveChangesAsync();
    }
}
