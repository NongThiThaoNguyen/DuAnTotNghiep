# Module M14 Publishing Workflow - Test Checklist

## Prerequisites
- [ ] Run all SQL migrations in order: m14_ai_module_check_and_create.sql → m14_seed_prompt_templates.sql → m14_add_batchid_to_ai_generated_contents.sql → m14_add_published_question_id.sql
- [ ] Set OpenAI:ApiKey in appsettings.json or User Secrets
- [ ] Ensure SQL Server database is created and accessible
- [ ] Create seed data: Skills, Topics, Proficiency Levels, User (with ADMIN/REVIEWER role)

## Test Flow 1: Single Content Publishing

### Step 1: Generate
- [ ] Navigate to `/QuizGeneration/Create`
- [ ] Select Skill → Topic cascades correctly
- [ ] Select Proficiency Level
- [ ] Select Question Type (MCQ, True/False, etc.)
- [ ] Enter Question Count (1-20)
- [ ] Click "Generate"
- [ ] Verify form redirects to `/QuizGeneration/Preview`
- [ ] View batch ID and PENDING items in preview table
- [ ] Check AiUsageLog entry created with status="SUCCESS"
- [ ] Check ai_generated_contents table: 
  - [ ] review_status = 'PENDING'
  - [ ] batch_id = guid
  - [ ] generated_content = valid JSON with question_text, options, correct_answer_index

### Step 2: Review
- [ ] Log in as ADMIN or REVIEWER user
- [ ] Navigate to `/Admin/AIContent/Index`
- [ ] Verify PENDING items appear in list
- [ ] Verify pending count by type badges displayed (QUESTION: X, QUIZ: Y)
- [ ] Apply filter by ContentType, TopicId - verify filter works
- [ ] Pagination: verify next/previous links work, numbered pages show

### Step 3: Inline Editing & Approval
- [ ] Click "Review" button on an item
- [ ] Verify Details page shows:
  - [ ] ID, ContentType, Topic, RequestedBy, CreatedAt, Status=PENDING
  - [ ] Question Text textarea (EDITABLE)
  - [ ] Options (read-only JSON)
  - [ ] Correct Answer Index (read-only)
  - [ ] Difficulty (read-only)
  - [ ] Explanation textarea (EDITABLE)
  - [ ] Skill Tags (read-only JSON)
  - [ ] Full Generated Content JSON (read-only)
  - [ ] Original Prompt (read-only, max-height scrollable)
- [ ] Edit question_text (change text)
- [ ] Edit explanation (add/modify text)
- [ ] Fill Copyright Check checkbox
- [ ] Select Plagiarism Risk (LOW/MEDIUM/HIGH)
- [ ] Enter Review Note (optional)
- [ ] Click "Approve" button
- [ ] Verify form captures edited values in hidden inputs
- [ ] Verify page redirects to details or shows success message
- [ ] Check database:
  - [ ] ai_generated_contents: review_status='APPROVED', reviewed_by=user_id, reviewed_at=now
  - [ ] content_compliance_reviews: new entry with copyright_check, plagiarism_risk, review_status='APPROVED'
  - [ ] audit_logs: new entry with action='APPROVE', old_value='PENDING', new_value='APPROVED'

### Step 4: Single Publish
- [ ] Navigate back to Details page for the APPROVED item
- [ ] Verify "📤 Publish to Question Bank" button appears (only for APPROVED status)
- [ ] Click Publish button
- [ ] Verify page redirects to `/Admin/AIContent/PublishResult`
- [ ] Verify success message: "Successfully published to Question Bank (ID: {question_id})"
- [ ] Verify section shows:
  - [ ] AI Content ID
  - [ ] Published Question ID (clickable link to view question)
  - [ ] No Draft Quiz Created (unless batch publish was used)
- [ ] Check database:
  - [ ] question_bank: new entry with source_type='AI_GENERATED', review_status='PUBLISHED'
  - [ ] question_bank.question_text = edited question (from inline edit)
  - [ ] question_bank.explanation = edited explanation
  - [ ] question_options: new entries for each option, with is_correct marked
  - [ ] ai_generated_contents: published_question_id = {question_id}

## Test Flow 2: Batch Publishing with Auto-Quiz

### Step 1: Generate Multiple
- [ ] Create 3-5 batches of questions via Generate endpoint
- [ ] Note batch IDs

### Step 2: Approve Multiple
- [ ] Go to `/Admin/AIContent/Index`
- [ ] Approve 3-5 items (mix of APPROVED and PENDING)
- [ ] Verify each gets APPROVED status, compliance review, audit log

### Step 3: Batch Publish with Quiz Creation
- [ ] On Index page, scroll to "Bulk Actions" section
- [ ] Enter Quiz Title: "Auto-Generated Practice Quiz"
- [ ] Check "Create Quiz" checkbox
- [ ] Click "📤 Publish All Approved"
- [ ] Verify page redirects to `/Admin/AIContent/PublishBatchResult`
- [ ] Verify success message shows:
  - [ ] Successfully published X items
  - [ ] Draft Quiz Created (ID: {quiz_id})
- [ ] Verify statistics:
  - [ ] Batch ID: ALL_APPROVED (or specific if filtered)
  - [ ] Published Count badge: X
  - [ ] No Failure Count (or Y if some had duplicates)
  - [ ] Draft Quiz link: "Edit Quiz (X questions)"
- [ ] Check database:
  - [ ] question_bank: X new entries with source_type='AI_GENERATED'
  - [ ] quiz: new entry with title='Auto-Generated Practice Quiz', status='DRAFT'
  - [ ] quiz_questions: X entries linking quiz_id to each question_id

## Test Flow 3: Error Cases

### Reject Content
- [ ] Navigate to Details of PENDING item
- [ ] Under "Reject" form, leave Review Note empty
- [ ] Click "Reject" button
- [ ] Verify validation error: "Review note is required for rejection"
- [ ] Enter rejection reason: "Poor grammar"
- [ ] Click "Reject"
- [ ] Verify page shows success message
- [ ] Check database: ai_generated_contents.review_status='REJECTED'
- [ ] Verify AuditLog entry: action='REJECT'

### Request Revision
- [ ] Navigate to Details of PENDING item
- [ ] Enter Revision Note: "Fix capitalization"
- [ ] Click "Request Revision"
- [ ] Verify success message
- [ ] Check database: ai_generated_contents.review_status='NEEDS_REVISION'
- [ ] Verify AuditLog entry: action='REQUEST_REVISION'

### Duplicate Detection
- [ ] Approve a question with text: "What is the capital of France?"
- [ ] Publish it to Question Bank
- [ ] Generate another question with identical text: "What is the capital of France?"
- [ ] Approve this new one
- [ ] Try to Publish
- [ ] Verify error: "A similar question already exists in the Question Bank for this topic."

### Already Published
- [ ] Try to Publish the same question again (refresh page)
- [ ] Verify publish button is hidden (no PublishedQuestionId badge already shown)
- [ ] Or verify error if manually POSTing: "This content has already been published."

## Test Flow 4: Audit Trail Verification

### View Audit Logs
- [ ] Navigate to Audit Logs view (if available in system)
- [ ] Filter by entity_name='ai_generated_contents'
- [ ] Verify entries for each action: APPROVE, REJECT, REQUEST_REVISION
- [ ] Verify fields:
  - [ ] action: APPROVE | REJECT | REQUEST_REVISION
  - [ ] old_value: PENDING
  - [ ] new_value: APPROVED | REJECTED | NEEDS_REVISION
  - [ ] user_id: reviewer ID
  - [ ] created_at: timestamp

## Test Flow 5: UI/UX Verification

### Publish Button Visibility
- [ ] On Details page, verify "Publish" button visible only when status=APPROVED
- [ ] For PENDING items: no button
- [ ] For REJECTED/NEEDS_REVISION: no button
- [ ] For APPROVED+unpublished: button visible
- [ ] For APPROVED+already published: badge shown "Published (Q#123)", no button

### Published Reference Display
- [ ] On Details page, show message: "✓ This content is APPROVED and can be published"
- [ ] If already published, show badge: "Published (Q#{id})"
- [ ] "View Question" link clickable if Question Bank detail route exists

### Batch Publish Results
- [ ] Results page shows clear statistics with badges
- [ ] If quiz created, show "Edit Quiz (X questions)" button
- [ ] Back buttons navigate correctly

## Performance/Stress Tests

- [ ] Publish 100 items in batch: verify transaction completes
- [ ] Publish with 10 options per question: verify question_options all created
- [ ] Duplicate detection on 1000 existing questions: verify performance acceptable

## Database Integrity

- [ ] Verify foreign key constraints:
  - [ ] ai_generated_contents.published_question_id → question_bank.id
  - [ ] ai_generated_contents.related_topic_id → learning_topics.id
  - [ ] content_compliance_reviews.content_id → ai_generated_contents.id
- [ ] Run integrity check: no orphaned references
- [ ] Transaction rollback test: introduce error during option insert, verify question_bank rollback

## Final Checklist

- [ ] All SQL migrations execute without error
- [ ] No compile errors in Visual Studio
- [ ] No runtime errors in browser console
- [ ] Audit trail fully populated
- [ ] All published items appear in Question Bank
- [ ] Quiz draft correctly references published questions
- [ ] Pagination works correctly
- [ ] Filters work correctly
- [ ] Inline editing preserves values correctly
- [ ] Duplicate detection prevents publishing same question twice
