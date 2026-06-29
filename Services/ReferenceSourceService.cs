using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Enums;
using DuAnTotNghiep.Repositories.Interfaces;
using DuAnTotNghiep.Services.Interfaces;
using DuAnTotNghiep.ViewModels.Admin.ReferenceSources;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services
{
    public class ReferenceSourceService : IReferenceSourceService
    {
        private readonly IReferenceSourceRepository _repository;
        private readonly IAuditService _auditService;
        private readonly ApplicationDbContext _context;
        private readonly IValidateLicenseService _licenseValidationService;

        public ReferenceSourceService(
            IReferenceSourceRepository repository,
            IAuditService auditService,
            ApplicationDbContext context,
            IValidateLicenseService licenseValidationService)
        {
            _repository = repository;
            _auditService = auditService;
            _context = context;
            _licenseValidationService = licenseValidationService;
        }

        public async Task<IEnumerable<ReferenceSource>> GetListAsync(ReferenceSourceType? sourceType, ReferenceReviewStatus? status, string? keyword)
        {
            return await _repository.GetListAsync(sourceType, status, keyword);
        }

        public async Task<ReferenceSource?> GetDetailsAsync(int id)
        {
            return await _repository.GetDetailsAsync(id);
        }

        public async Task<int> CreateAsync(ReferenceSource source, int userId)
        {
            if (string.IsNullOrWhiteSpace(source.SourceName))
                throw new InvalidOperationException("Tên nguồn tham khảo không được để trống.");

            ValidateUrl(source.SourceUrl);

            if (await _repository.ExistsByNameAsync(source.SourceName))
                throw new InvalidOperationException("Tên nguồn tham khảo này đã tồn tại.");

            if (!string.IsNullOrWhiteSpace(source.SourceUrl) && await _repository.ExistsByUrlAsync(source.SourceUrl))
                throw new InvalidOperationException("Địa chỉ URL nguồn tham khảo này đã tồn tại.");

            bool isExternal = source.SourceType == ReferenceSourceType.OFFICIAL || 
                              source.SourceType == ReferenceSourceType.OPEN_LICENSE || 
                              source.SourceType == ReferenceSourceType.REFERENCE_ONLY;

            if (isExternal)
            {
                if (string.IsNullOrWhiteSpace(source.SourceUrl))
                    throw new InvalidOperationException("Địa chỉ URL là bắt buộc đối với nguồn tham khảo bên ngoài (External).");
            }

            await _licenseValidationService.ValidateLicenseAsync(source);

            source.CreatedAt = DateTime.UtcNow;
            source.UpdatedAt = DateTime.UtcNow;
            source.CreatedBy = userId;
            source.IsActive = true;
            source.Status = ReferenceReviewStatus.PENDING;

            await _repository.CreateAsync(source);
            await _repository.SaveChangesAsync();

            await _auditService.LogAsync(userId, "Create Reference Source", "ReferenceSource", source.Id, null, source.SourceName);

            return source.Id;
        }

        public async Task<EditReferenceSourceViewModel?> GetEditModelAsync(int id, int currentUserId, string userRole)
        {
            var roleUpper = userRole.ToUpper();
            if (roleUpper != "ADMIN" && roleUpper != "TEACHER")
            {
                throw new UnauthorizedAccessException("Bạn không có quyền truy cập thông tin nguồn tham khảo này.");
            }

            var existing = await _repository.GetDetailsAsync(id);
            if (existing == null) return null;

            if (existing.Status == ReferenceReviewStatus.ARCHIVED)
            {
                throw new InvalidOperationException("Không thể chỉnh sửa nguồn tham khảo đã được lưu trữ (Archived). Vui lòng khôi phục trước.");
            }

            if (roleUpper == "TEACHER" && existing.CreatedBy != currentUserId)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền chỉnh sửa nguồn tham khảo của người khác.");
            }

            return new EditReferenceSourceViewModel
            {
                Id = existing.Id,
                SourceName = existing.SourceName,
                SourceUrl = existing.SourceUrl,
                ComplianceEvidenceUrl = existing.ComplianceEvidenceUrl,
                SourceType = existing.SourceType,
                Author = existing.Author,
                Organization = existing.Organization,
                Description = existing.Description,
                LicenseNote = existing.LicenseNote,
                UsagePolicy = existing.UsagePolicy,
                Status = existing.Status,
                IsActive = existing.IsActive
            };
        }

        public async Task UpdateAsync(EditReferenceSourceViewModel model, int currentUserId, string userRole)
        {
            var roleUpper = userRole.ToUpper();
            if (roleUpper != "ADMIN" && roleUpper != "TEACHER")
            {
                throw new UnauthorizedAccessException("Bạn không có quyền thực hiện hành động này.");
            }

            var existing = await _repository.GetDetailsAsync(model.Id);
            if (existing == null)
                throw new InvalidOperationException("Nguồn tham khảo không tồn tại.");

            if (existing.Status == ReferenceReviewStatus.ARCHIVED)
            {
                throw new InvalidOperationException("Không thể chỉnh sửa nguồn tham khảo đã được lưu trữ (Archived). Vui lòng khôi phục trước.");
            }

            if (roleUpper == "TEACHER" && existing.CreatedBy != currentUserId)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền chỉnh sửa nguồn tham khảo của người khác.");
            }

            if (string.IsNullOrWhiteSpace(model.SourceName))
                throw new InvalidOperationException("Tên nguồn tham khảo không được để trống.");

            ValidateUrl(model.SourceUrl);

            if (await _repository.ExistsByNameAsync(model.SourceName, model.Id))
                throw new InvalidOperationException("Tên nguồn tham khảo này đã tồn tại.");

            if (!string.IsNullOrWhiteSpace(model.SourceUrl) && await _repository.ExistsByUrlAsync(model.SourceUrl, model.Id))
                throw new InvalidOperationException("Địa chỉ URL nguồn tham khảo này đã tồn tại.");

            bool isExternal = model.SourceType == ReferenceSourceType.OFFICIAL || 
                              model.SourceType == ReferenceSourceType.OPEN_LICENSE || 
                              model.SourceType == ReferenceSourceType.REFERENCE_ONLY;

            if (isExternal)
            {
                if (string.IsNullOrWhiteSpace(model.SourceUrl))
                    throw new InvalidOperationException("Địa chỉ URL là bắt buộc đối với nguồn tham khảo bên ngoài (External).");
            }

            // Chặn xóa license_note khi nguồn đã được APPROVED hoặc ARCHIVED
            bool wasApprovedOrArchived = existing.Status == ReferenceReviewStatus.APPROVED || existing.Status == ReferenceReviewStatus.ARCHIVED;
            if (wasApprovedOrArchived && isExternal && string.IsNullOrWhiteSpace(model.LicenseNote))
            {
                throw new InvalidOperationException("Không được xóa ghi chú bản quyền (License Note) khi nguồn đã được phê duyệt hoặc lưu trữ.");
            }

            bool isCriticalFieldChanged = existing.SourceUrl != model.SourceUrl ||
                                          existing.LicenseNote != model.LicenseNote ||
                                          existing.UsagePolicy != model.UsagePolicy ||
                                          existing.SourceType != model.SourceType ||
                                          existing.ComplianceEvidenceUrl != model.ComplianceEvidenceUrl;

            var oldStatus = existing.Status;
            var oldName = existing.SourceName;

            existing.SourceName = model.SourceName.Trim();
            existing.SourceUrl = model.SourceUrl?.Trim();
            existing.SourceType = model.SourceType;
            existing.Author = model.Author?.Trim();
            existing.Organization = model.Organization?.Trim();
            existing.Description = model.Description?.Trim();
            existing.LicenseNote = model.LicenseNote?.Trim();
            existing.UsagePolicy = model.UsagePolicy;
            existing.ComplianceEvidenceUrl = model.ComplianceEvidenceUrl?.Trim();
            existing.IsActive = model.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            await _licenseValidationService.ValidateLicenseAsync(existing);

            if (existing.Status == ReferenceReviewStatus.APPROVED && isCriticalFieldChanged)
            {
                existing.Status = ReferenceReviewStatus.PENDING;
                existing.ApprovedBy = null;
                existing.ApprovedAt = null;
            }

            await _repository.UpdateAsync(existing);
            await _repository.SaveChangesAsync();

            var changesMsg = $"Cập nhật nguồn tham khảo: {oldName} -> {existing.SourceName}.";
            if (oldStatus != existing.Status)
            {
                changesMsg += $" Trạng thái tự động đổi từ {oldStatus} thành {existing.Status} do sửa đổi thông tin quan trọng.";
            }

            await _auditService.LogAsync(currentUserId, "Update Reference Source", "ReferenceSource", existing.Id, oldStatus.ToString(), existing.Status.ToString() + " - " + changesMsg);
        }

        public async Task ApproveAsync(int id, int adminId, string userRole, string? note = null)
        {
            EnsureAdmin(userRole);

            var existing = await _repository.GetDetailsAsync(id);
            if (existing == null)
                throw new InvalidOperationException("Nguồn tham khảo không tồn tại.");

            if (existing.Status == ReferenceReviewStatus.ARCHIVED)
                throw new InvalidOperationException("Không thể phê duyệt nguồn tham khảo đã được lưu trữ (Archived).");

            await _licenseValidationService.ValidateLicenseAsync(existing);

            if (!string.IsNullOrWhiteSpace(existing.SourceUrl))
            {
                ValidateUrl(existing.SourceUrl);
            }

            var oldStatus = existing.Status.ToString();

            existing.Status = ReferenceReviewStatus.APPROVED;
            existing.ApprovedBy = adminId;
            existing.ApprovedAt = DateTime.UtcNow;
            existing.UpdatedAt = DateTime.UtcNow;

            existing.RejectedBy = null;
            existing.RejectedAt = null;
            existing.RejectionReason = null;

            await _repository.UpdateAsync(existing);

            var complianceReview = new ContentComplianceReview
            {
                ContentType = "ReferenceSource",
                ContentId = id,
                ReferenceSourceId = id,
                ReviewerId = adminId,
                CopyrightCheck = true,
                PlagiarismRisk = "LOW",
                ReviewStatus = "APPROVED",
                ReviewNote = note,
                ReviewedAt = DateTime.UtcNow
            };
            await _repository.AddComplianceReviewAsync(complianceReview);

            await _repository.SaveChangesAsync();

            await _auditService.LogAsync(adminId, "Approve Reference Source", "ReferenceSource", id, oldStatus, "APPROVED");
        }

        public async Task RejectAsync(int id, string reason, int adminId, string userRole)
        {
            EnsureAdmin(userRole);

            if (string.IsNullOrWhiteSpace(reason))
                throw new InvalidOperationException("Lý do từ chối bắt buộc phải có.");

            var existing = await _repository.GetDetailsAsync(id);
            if (existing == null)
                throw new InvalidOperationException("Nguồn tham khảo không tồn tại.");

            if (existing.Status == ReferenceReviewStatus.ARCHIVED)
                throw new InvalidOperationException("Không thể từ chối nguồn tham khảo đã được lưu trữ (Archived).");

            var oldStatus = existing.Status.ToString();

            existing.Status = ReferenceReviewStatus.REJECTED;
            existing.RejectedBy = adminId;
            existing.RejectedAt = DateTime.UtcNow;
            existing.RejectionReason = reason;
            existing.UpdatedAt = DateTime.UtcNow;

            existing.ApprovedBy = null;
            existing.ApprovedAt = null;

            await _repository.UpdateAsync(existing);

            var complianceReview = new ContentComplianceReview
            {
                ContentType = "ReferenceSource",
                ContentId = id,
                ReferenceSourceId = id,
                ReviewerId = adminId,
                CopyrightCheck = false,
                PlagiarismRisk = "HIGH",
                ReviewStatus = "REJECTED",
                ReviewNote = reason,
                ReviewedAt = DateTime.UtcNow
            };
            await _repository.AddComplianceReviewAsync(complianceReview);

            await _repository.SaveChangesAsync();

            var newStatusWithNote = $"REJECTED - Lý do: {reason.Trim()}";
            await _auditService.LogAsync(adminId, "Reject Reference Source", "ReferenceSource", id, oldStatus, newStatusWithNote);
        }

        public async Task ArchiveAsync(int id, int userId, string userRole)
        {
            EnsureAdmin(userRole);

            var existing = await _repository.GetDetailsAsync(id);
            if (existing == null)
                throw new InvalidOperationException("Nguồn tham khảo không tồn tại.");

            bool isUsedInTopic = await _repository.IsUsedInTopicAsync(id);
            bool isUsedInLesson = await _repository.IsUsedInLessonAsync(id);
            bool isUsedInAiWorkflow = await _repository.IsUsedInAiWorkflowAsync(id);

            if (isUsedInTopic || isUsedInLesson || isUsedInAiWorkflow)
            {
                var msg = "Không thể Lưu trữ (Archive) nguồn tham khảo này vì đang được sử dụng:\n";
                if (isUsedInTopic) msg += "- Có học phần (Topic) đang liên kết.\n";
                if (isUsedInLesson) msg += "- Có bài học (Lesson) thuộc học phần liên kết.\n";
                if (isUsedInAiWorkflow) msg += "- Có nội dung AI (AI Workflow) thuộc học phần liên kết.\n";
                throw new InvalidOperationException(msg);
            }

            existing.Status = ReferenceReviewStatus.ARCHIVED;
            existing.IsActive = false;
            existing.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(existing);
            await _repository.SaveChangesAsync();

            await _auditService.LogAsync(userId, "Archive Reference Source", "ReferenceSource", id, "Active", "Archived");
        }

        public async Task<ReferenceSourcePagedViewModel> GetPagedListAsync(string? keyword, ReferenceSourceType? sourceType, ReferenceReviewStatus? status, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            keyword = keyword?.Trim();

            if (sourceType.HasValue && !Enum.IsDefined(typeof(ReferenceSourceType), sourceType.Value))
                throw new InvalidOperationException("Loại nguồn tham khảo tìm kiếm không hợp lệ.");

            if (status.HasValue && !Enum.IsDefined(typeof(ReferenceReviewStatus), status.Value))
                throw new InvalidOperationException("Trạng thái kiểm duyệt tìm kiếm không hợp lệ.");

            var (items, totalItems) = await _repository.GetPagedListAsync(keyword, sourceType, status, page, pageSize);

            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            if (totalPages > 0 && page > totalPages)
            {
                page = totalPages;
                var correctedData = await _repository.GetPagedListAsync(keyword, sourceType, status, page, pageSize);
                items = correctedData.Items;
            }

            var statusSummary = await _repository.CountByStatusAsync();

            return new ReferenceSourcePagedViewModel
            {
                Items = items,
                TotalItems = totalItems,
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                Keyword = keyword,
                SourceType = sourceType,
                Status = status,
                StatusSummary = statusSummary
            };
        }

        public async Task<ReferenceSourceDetailsViewModel?> GetDetailsViewModelAsync(int id, int currentUserId, string userRole)
        {
            var roleUpper = userRole.ToUpper();
            if (roleUpper != "ADMIN" && roleUpper != "TEACHER")
            {
                throw new UnauthorizedAccessException("Bạn không có quyền truy cập thông tin nguồn tham khảo này.");
            }

            var source = await _repository.GetDetailsAsync(id);
            if (source == null) return null;

            if (source.Status == ReferenceReviewStatus.ARCHIVED && roleUpper != "ADMIN")
            {
                throw new UnauthorizedAccessException("Nguồn tham khảo đã được lưu trữ và chỉ có thể truy cập bởi Quản trị viên.");
            }

            var linkedLessons = await _repository.GetLinkedLessonsAsync(id);
            var auditLogs = await _repository.GetRecentAuditLogsAsync(id, 10);

            bool canEdit = (roleUpper == "ADMIN") || (roleUpper == "TEACHER" && source.Status == ReferenceReviewStatus.DRAFT);
            bool canReview = (roleUpper == "ADMIN") && (source.Status == ReferenceReviewStatus.PENDING);
            bool canArchive = (roleUpper == "ADMIN") && (source.Status != ReferenceReviewStatus.ARCHIVED);
            bool canSubmit = (roleUpper == "ADMIN" || (roleUpper == "TEACHER" && source.CreatedBy == currentUserId))
                             && (source.Status == ReferenceReviewStatus.DRAFT || source.Status == ReferenceReviewStatus.REJECTED);

            var model = new ReferenceSourceDetailsViewModel
            {
                Id = source.Id,
                SourceName = source.SourceName,
                SourceUrl = source.SourceUrl,
                SourceType = source.SourceType,
                Author = source.Author,
                Organization = source.Organization,
                Description = source.Description,
                LicenseNote = source.LicenseNote,
                UsagePolicy = source.UsagePolicy,
                Status = source.Status,
                CreatedById = source.CreatedBy,
                CreatedByUserName = source.CreatedByNavigation != null ? source.CreatedByNavigation.FullName : "Hệ thống",
                CreatedAt = source.CreatedAt,
                ApprovedById = source.ApprovedBy,
                ApprovedByUserName = source.ApprovedByNavigation != null ? source.ApprovedByNavigation.FullName : "Chưa phê duyệt",
                ApprovedAt = source.ApprovedAt,
                IsActive = source.IsActive,
                CanEdit = canEdit,
                CanReview = canReview,
                CanArchive = canArchive,
                CanSubmit = canSubmit,
                LinkedTopics = source.TopicReferences.Select(tr => new TopicReferenceDto
                {
                    Id = tr.TopicId,
                    TopicCode = tr.Topic != null ? tr.Topic.TopicCode : string.Empty,
                    Title = tr.Topic != null ? tr.Topic.Title : string.Empty,
                    ReferenceNote = tr.Note
                }).ToList(),
                LinkedLessons = linkedLessons.Select(l => new LessonReferenceDto
                {
                    Id = l.Id,
                    Title = l.Title,
                    TopicTitle = l.Topic != null ? l.Topic.Title : string.Empty
                }).ToList(),
                RecentAuditLogs = auditLogs.Select(a => new AuditLogDto
                {
                    Id = a.Id,
                    Action = a.Action,
                    ActorName = a.User != null ? a.User.FullName : "Hệ thống",
                    CreatedAt = a.CreatedAt,
                    OldValue = a.OldValue,
                    NewValue = a.NewValue
                }).ToList()
            };

            return model;
        }

        public async Task SubmitForReviewAsync(int id, int currentUserId, string userRole)
        {
            var roleUpper = userRole.ToUpper();
            if (roleUpper != "ADMIN" && roleUpper != "TEACHER")
            {
                throw new UnauthorizedAccessException("Bạn không có quyền thực hiện hành động này.");
            }

            var existing = await _repository.GetDetailsAsync(id);
            if (existing == null)
                throw new InvalidOperationException("Nguồn tham khảo không tồn tại.");

            if (roleUpper == "TEACHER" && existing.CreatedBy != currentUserId)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền gửi duyệt nguồn tham khảo của người khác.");
            }

            if (existing.Status == ReferenceReviewStatus.ARCHIVED)
            {
                throw new InvalidOperationException("Không thể gửi duyệt nguồn tham khảo đã được lưu trữ (Archived).");
            }
            if (existing.Status == ReferenceReviewStatus.APPROVED)
            {
                throw new InvalidOperationException("Nguồn tham khảo này đã được phê duyệt.");
            }
            if (existing.Status != ReferenceReviewStatus.DRAFT && existing.Status != ReferenceReviewStatus.REJECTED)
            {
                throw new InvalidOperationException($"Trạng thái nguồn tham khảo không hợp lệ để gửi duyệt. Trạng thái hiện tại: {existing.Status}.");
            }

            if (string.IsNullOrWhiteSpace(existing.SourceName))
                throw new InvalidOperationException("Tên nguồn tham khảo không được để trống.");

            if (string.IsNullOrWhiteSpace(existing.SourceUrl))
                throw new InvalidOperationException("Địa chỉ URL nguồn tham khảo là bắt buộc trước khi gửi duyệt.");

            ValidateUrl(existing.SourceUrl);

            bool isExternal = existing.SourceType == ReferenceSourceType.OFFICIAL || 
                              existing.SourceType == ReferenceSourceType.OPEN_LICENSE || 
                              existing.SourceType == ReferenceSourceType.REFERENCE_ONLY;

            if (isExternal && string.IsNullOrWhiteSpace(existing.LicenseNote))
            {
                throw new InvalidOperationException("Ghi chú bản quyền (License Note) là bắt buộc đối với nguồn tham khảo bên ngoài (External).");
            }

            var oldStatus = existing.Status;

            existing.Status = ReferenceReviewStatus.PENDING;
            existing.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(existing);
            await _repository.SaveChangesAsync();

            await _auditService.LogAsync(currentUserId, "Submit Review", "ReferenceSource", id, oldStatus.ToString(), "PENDING");
        }

        public async Task LinkSourceToTopicAsync(int topicId, int sourceId, int userId, string? note = null)
        {
            var topic = await _context.LearningTopics.FindAsync(topicId);
            if (topic == null)
                throw new InvalidOperationException("Học phần không tồn tại.");

            bool isActive = topic.Status.Equals("Active", StringComparison.OrdinalIgnoreCase) || 
                            topic.Status.Equals("ACTIVE", StringComparison.OrdinalIgnoreCase);
            if (!isActive)
                throw new InvalidOperationException("Chỉ được liên kết nguồn tham khảo với học phần đang hoạt động (Active).");

            var source = await _repository.GetDetailsAsync(sourceId);
            if (source == null)
                throw new InvalidOperationException("Nguồn tham khảo không tồn tại.");

            if (source.Status != ReferenceReviewStatus.APPROVED)
                throw new InvalidOperationException("Chỉ được phép liên kết nguồn tham khảo đã được phê duyệt (Approved).");

            bool exists = await _context.TopicReferences.AnyAsync(tr => tr.TopicId == topicId && tr.ReferenceSourceId == sourceId);
            if (exists)
                throw new InvalidOperationException("Nguồn tham khảo này đã được liên kết với học phần trước đó.");

            var reference = new TopicReference
            {
                TopicId = topicId,
                ReferenceSourceId = sourceId,
                Note = note?.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            await _context.TopicReferences.AddAsync(reference);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(userId, "Link Reference Source to Topic", "TopicReference", reference.Id, null, $"Linked Source {sourceId} to Topic {topicId}");
        }

        public async Task UnlinkSourceFromTopicAsync(int topicId, int sourceId, int userId)
        {
            var reference = await _context.TopicReferences
                .FirstOrDefaultAsync(tr => tr.TopicId == topicId && tr.ReferenceSourceId == sourceId);

            if (reference == null)
                throw new InvalidOperationException("Liên kết giữa nguồn tham khảo và học phần không tồn tại.");

            _context.TopicReferences.Remove(reference);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(userId, "Unlink Reference Source from Topic", "TopicReference", reference.Id, $"Topic {topicId} - Source {sourceId}", null);
        }

        public async Task<LicenseSummaryResult> GetLicenseSummaryAsync(int sourceId)
        {
            var source = await _repository.GetDetailsAsync(sourceId);
            if (source == null)
            {
                throw new InvalidOperationException("Nguồn tham khảo không tồn tại.");
            }

            var warningLevel = "NONE";
            if (source.UsagePolicy == ReferenceUsagePolicy.RESTRICTED)
            {
                warningLevel = "WARNING";
            }
            else if (source.UsagePolicy == ReferenceUsagePolicy.REFERENCE_ONLY)
            {
                warningLevel = "BLOCK_INFO";
            }

            return new LicenseSummaryResult
            {
                LicenseNote = source.LicenseNote ?? string.Empty,
                UsagePolicy = source.UsagePolicy?.ToString() ?? string.Empty,
                WarningLevel = warningLevel
            };
        }

        private void ValidateUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return;

            if (!Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uriResult) 
                || (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
            {
                throw new InvalidOperationException("Địa chỉ URL không hợp lệ. Vui lòng nhập URL dạng HTTP hoặc HTTPS.");
            }
        }

        private void EnsureAdmin(string userRole)
        {
            if (string.IsNullOrWhiteSpace(userRole) || userRole.ToUpper() != "ADMIN")
            {
                throw new UnauthorizedAccessException("Bạn không có quyền thực hiện hành động này. Chỉ Quản trị viên mới được phép.");
            }
        }
    }
}
