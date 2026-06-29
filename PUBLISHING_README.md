# Module M14: AI-Generated Content Publishing to Question Bank

## Overview

This feature provides a complete workflow to review AI-generated questions and publish them to the official Question Bank.

**Workflow:** Generate → Review (Approve/Reject/Revise) → Publish → Quiz

---

## Architecture

### Service Layer

#### `IPublishingService` & `PublishingService`
- **PublishToQuestionBankAsync(aiContentId, reviewedByUserId)**
  - Validates content is APPROVED
  - Detects duplicates by normalized question text
  - Starts transaction for atomicity
  - Inserts question_bank entry with AI_GENERATED source
  - Inserts question_options for MCQ
  - Updates ai_generated_contents.published_question_id
  - Rollback on error (e.g., option insert fails)
  - Returns: PublishResult with question_id, message

- **PublishBatchAsync(batchId?, publishedByUserId, createQuizDraft?, quizTitle?)**
  - Publishes all APPROVED items (filtered by batchId if provided)
  - Calls PublishToQuestionBankAsync for each item
  - Counts successes/failures
  - Optionally creates Quiz draft with all published questions
  - Returns: PublishBatchResult with counts, quiz_id

#### `AIContentReviewService` (Enhanced)
- GetContentDetailAsync now includes PublishedQuestionId
- All review actions (Approve/Reject/RequestRevision) create AuditLog entries

### Controllers

#### `AIContentController` (Admin Area)
```
Routes:
  GET  /Admin/AIContent/Index              → List PENDING items
  GET  /Admin/AIContent/Details/{id}       → Review detail + inline edit
  POST /Admin/AIContent/Approve/{id}       → Set APPROVED (with edit capture)
  POST /Admin/AIContent/Reject/{id}        → Set REJECTED (require note)
  POST /Admin/AIContent/RequestRevision/{id} → Set NEEDS_REVISION
  
  POST /Admin/AIContent/Publish/{id}           → Publish single to Question Bank
  GET  /Admin/AIContent/PublishResult          → Result page for single publish
  
  POST /Admin/AIContent/PublishBatch           → Batch publish all APPROVED
  GET  /Admin/AIContent/PublishBatchResult     → Result page for batch publish
```

### Models

**AiGeneratedContent** (Enhanced)
```csharp
public int? PublishedQuestionId { get; set; }
public virtual QuestionBank? PublishedQuestion { get; set; }
```

**QuestionBank** (Target for publishing)
```csharp
public string SourceType { get; set; }        // AI_GENERATED | MANUAL | IMPORTED
public string ReviewStatus { get; set; }      // PUBLISHED | DRAFT | etc.
```

**AuditLog** (Track review changes)
```csharp
public string Action { get; set; }            // APPROVE | REJECT | REQUEST_REVISION
public string? OldValue { get; set; }         // PENDING
public string? NewValue { get; set; }         // APPROVED | REJECTED | NEEDS_REVISION
```

**ContentComplianceReview** (Record compliance decisions)
```csharp
public bool CopyrightCheck { get; set; }
public string? PlagiarismRisk { get; set; }   // LOW | MEDIUM | HIGH
public string ReviewNote { get; set; }
```

### ViewModels

**AiContentReviewDetailViewModel**
- Added: `bool IsApproved` (computed property)
- Added: `int? PublishedQuestionId`

**PublishResultViewModel**
- IsSuccess, Message, ErrorMessage
- PublishedQuestionId, CreatedQuizId
- ViewQuestionLink, ViewQuizLink

**PublishBatchResultViewModel**
- IsSuccess, PublishedCount, FailureCount
- CreatedQuizId, Message, ErrorMessage
- BatchId

### Views

**Details.cshtml** (Enhanced with Publish)
- Green alert when APPROVED: "This content is APPROVED and can be published"
- Badge if already published: "Published (Q#{id})"
- Publish button only visible for APPROVED items
- Form to capture edited question_text/explanation into hidden inputs
- All 3 review forms (Approve, RequestRevision, Reject)

**PublishResult.cshtml** (Single publish result)
- Success/failure alert
- Published Item Details: AI Content ID, Question ID, Draft Quiz (if created)
- Links to view question, edit quiz
- Back/navigation buttons

**PublishBatchResult.cshtml** (Batch publish result)
- Success/warning alert
- Batch Statistics: Batch ID, Published Count, Failure Count, Draft Quiz
- Links to open new quiz
- Back/navigation buttons

**Index.cshtml** (Enhanced with bulk actions)
- "Bulk Actions" section with input for quiz title
- Checkbox "Create Quiz"
- Button "📤 Publish All Approved"
- Text: "Publishes all APPROVED items to Question Bank"

---

## Database Schema

### New Column
```sql
ALTER TABLE ai_generated_contents
ADD published_question_id INT NULL;

ALTER TABLE ai_generated_contents
ADD CONSTRAINT FK_AiGeneratedContent_PublishedQuestion 
FOREIGN KEY (published_question_id) REFERENCES question_bank(id);
```

### Affected Tables
- **ai_generated_contents**: published_question_id (FK), review_status (PENDING → APPROVED)
- **question_bank**: new rows with source_type='AI_GENERATED', review_status='PUBLISHED'
- **question_options**: new rows for each option of published question
- **content_compliance_reviews**: new entry on Approve (copyright, plagiarism checks)
- **audit_logs**: new entry on status change (APPROVE/REJECT/REQUEST_REVISION)
- **quizzes**: optional new entry if batch publish with createQuizDraft=true

---

## Key Features

### ✓ Transactions & Atomicity
```csharp
using var transaction = await _db.Database.BeginTransactionAsync();
try {
    // Insert question_bank
    // Insert question_options
    // Update ai_generated_contents
    await transaction.CommitAsync();
} catch {
    await transaction.RollbackAsync();
}
```

### ✓ Duplicate Detection
- Normalizes question text: lowercase + trim spaces
- Searches for exact match in question_bank for same topic
- Prevents duplicate questions in Question Bank

### ✓ Inline Editing Before Publish
- Reviewer can edit question_text and explanation on Details page
- Edited values captured into hidden form fields
- Published question_bank entry contains edited values

### ✓ Comprehensive Audit Trail
- Every status change (APPROVE/REJECT/REQUEST_REVISION) logged
- AuditLog: EntityName, EntityId, Action, UserId, OldValue, NewValue, CreatedAt
- ContentComplianceReview: CopyrightCheck, PlagiarismRisk, ReviewNote

### ✓ Batch Operations
- Publish multiple items at once
- Optional auto-create Quiz draft with published items
- Returns success/failure counts

### ✓ Error Handling
- Returns detailed errors (not approved, duplicate, already published)
- Transaction rollback prevents partial publishes
- User-friendly error messages in result page

---

## Usage Examples

### Single Publish
```
1. Click "Publish to Question Bank" on Details (APPROVED item)
2. See PublishResult page with question_id
3. Item now in question_bank table with source_type='AI_GENERATED'
```

### Batch Publish
```
1. Approve 5 items on Index
2. Go to Bulk Actions, enter "Quiz Title", check "Create Quiz"
3. Click "Publish All Approved"
4. See PublishBatchResult: "Published 5 items, created Quiz (ID: 123)"
5. Quiz#123 is DRAFT status in quizzes table with 5 questions linked
```

### Rejection & Revision
```
1. Click "Reject" with reason: "Poor quality"
   → AuditLog: action=REJECT, old=PENDING, new=REJECTED
   → Cannot publish (not APPROVED)

2. Click "Request Revision" with note: "Fix capitalization"
   → AuditLog: action=REQUEST_REVISION, old=PENDING, new=NEEDS_REVISION
   → Cannot publish (not APPROVED)
```

---

## Security

- `[Authorize(Roles = "ADMIN,REVIEWER")]` on all review/publish actions
- User ID captured from ClaimTypes.NameIdentifier
- Audit trail tracks who did what and when
- Compliance review records copyright/plagiarism decisions

---

## Testing

See [TEST_CHECKLIST_M14_PUBLISHING.md](../TEST_CHECKLIST_M14_PUBLISHING.md) for comprehensive test flows covering:
- Single publish
- Batch publish with quiz creation
- Error cases (duplicate, already published, not approved)
- Inline editing
- Audit trail verification
- UI/UX verification

---

## Dependencies

- EntityFrameworkCore (for transaction)
- System.Text.Json (for JSON parsing)
- IPublishingService (registered in Program.cs)
- IAIContentReviewService (for GetContentDetailAsync enhancement)

---

## Future Enhancements

- [ ] Bulk approve/reject on Index page
- [ ] Export pending items as CSV
- [ ] Email notifications on publish
- [ ] Question difficulty/skill auto-mapping
- [ ] Levenshtein distance for fuzzy duplicate detection
- [ ] Approve without editing (faster workflow)
- [ ] Publish schedule (delay publish until specific date)
