# Tasks: Document Upload and Management

**Input**: Design documents from `/specs/001-document-upload-management/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Maps the task to the associated user story (e.g., US1, US2, US3, US4)
- Include exact file paths in descriptions

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Establish the project structure and local configuration required for the document feature.

- [ ] T001 Create feature directory structure and confirm the document implementation path under `specs/001-document-upload-management/`
- [ ] T002 [P] Add document storage configuration placeholders in `ContosoDashboard/appsettings.json` and `ContosoDashboard/appsettings.Development.json`
- [ ] T003 [P] Create the local upload directory structure under `ContosoDashboard/AppData/uploads/` and document the secure storage path in the project setup notes

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure required before any document story can be implemented.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [ ] T004 Add the `Document`, `DocumentShare`, `DocumentAccessLog`, and `ScanJobMessage` data model definitions in `ContosoDashboard/Models/Document.cs`, `ContosoDashboard/Models/DocumentShare.cs`, `ContosoDashboard/Models/DocumentAccessLog.cs`, and `ContosoDashboard/Models/ScanJobMessage.cs`
- [ ] T005 [P] Extend `ContosoDashboard/Data/ApplicationDbContext.cs` with document DbSets, indexes, and relationships for project/task/user/document access tracking
- [ ] T006 [P] Define the storage abstraction in `ContosoDashboard/Services/IFileStorageService.cs` and implement the local filesystem version in `ContosoDashboard/Services/LocalFileStorageService.cs`
- [ ] T007 Define the document service contract and core authorization logic in `ContosoDashboard/Services/IDocumentService.cs` and `ContosoDashboard/Services/DocumentService.cs`
- [ ] T008 [P] Add the Azure queue integration contract in `ContosoDashboard/Services/IScanQueueService.cs` and `ContosoDashboard/Services/AzureQueueScanService.cs` for future async virus scanning
- [ ] T009 Register the document and storage services in `ContosoDashboard/Program.cs` and ensure authentication/authorization configuration remains aligned with the current mock identity model

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel.

---

## Phase 3: User Story 1 - Upload and organize work documents (Priority: P1) 🎯 MVP

**Goal**: Users can upload supported documents with metadata and see them in the right project or personal context.

**Independent Test**: A signed-in user can upload a valid file, receive validation feedback for invalid files, and confirm the document appears in the correct document list without exposing it to unauthorized users.

### Implementation for User Story 1

- [ ] T010 [P] [US1] Build the document list and upload UI shell in `ContosoDashboard/Pages/Documents.razor`
- [ ] T011 [P] [US1] Add metadata capture and validation logic for title, category, project assignment, and file size/type checks in `ContosoDashboard/Pages/Documents.razor`
- [ ] T012 [US1] Implement the upload workflow in `ContosoDashboard/Services/DocumentService.cs`, including safe path generation, metadata persistence, and scan-queue enqueue behavior
- [ ] Task 12: Implement `DocumentController` `POST /api/documents` endpoint in `ContosoDashboard/Controllers/DocumentController.cs`
  - Depends on: Task 11 (DocumentService)
  - Note: Include comprehensive error handling for file size limits and unsupported types
- [ ] T013 [US1] Implement local file persistence in `ContosoDashboard/Services/LocalFileStorageService.cs` using GUID-based filenames and a path pattern outside `wwwroot`
- [ ] T014 [US1] Update the project detail view to surface project documents in `ContosoDashboard/Pages/ProjectDetails.razor`
- [ ] T015 [US1] Add the document status and lifecycle handling needed for queued or pending scans in `ContosoDashboard/Models/Document.cs` and `ContosoDashboard/Services/DocumentService.cs`

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently.

---

## Phase 4: User Story 2 - Search and access the right document at the right time (Priority: P2)

**Goal**: Users can find and open only the documents they are authorized to access.

**Independent Test**: A user searches by title, description, tag, or project name and is only shown authorized documents; unauthorized direct access is denied.

### Implementation for User Story 2

- [ ] T016 [P] [US2] Implement document list queries, sort, and filter logic in `ContosoDashboard/Services/DocumentService.cs`
- [ ] T017 [P] [US2] Add search and filter controls to `ContosoDashboard/Pages/Documents.razor`
- [ ] T018 [US2] Enforce per-document authorization checks for view, download, and preview actions in `ContosoDashboard/Services/DocumentService.cs`
- [ ] T019 [US2] Add access-safe download and preview UX in `ContosoDashboard/Pages/Documents.razor` and `ContosoDashboard/Pages/ProjectDetails.razor`
- [ ] T020 [US2] Ensure unauthorized direct access attempts are rejected through the service layer and not hidden by the UI in `ContosoDashboard/Services/DocumentService.cs`

**Checkpoint**: At this point, User Stories 1 and 2 should both work independently.

---

## Phase 5: User Story 3 - Share and track document activity across teams (Priority: P2)

**Goal**: Document owners can share items with teammates and administrators can review access activity.

**Independent Test**: A user shares a document with another team member, the recipient sees it in the shared section, and the document activity is logged.

### Implementation for User Story 3

- [ ] T021 [P] [US3] Add the share model and relation for explicit access grants in `ContosoDashboard/Models/DocumentShare.cs` and `ContosoDashboard/Data/ApplicationDbContext.cs`
- [ ] T022 [US3] Implement share creation and recipient validation logic in `ContosoDashboard/Services/DocumentService.cs`
- [ ] T023 [US3] Add notification creation for document shares in `ContosoDashboard/Services/NotificationService.cs`
- [ ] T024 [US3] Add the audit log model and write logic in `ContosoDashboard/Models/DocumentAccessLog.cs` and `ContosoDashboard/Services/DocumentService.cs`
- [ ] T025 [US3] Surface shared-document activity in the document pages and notification center using `ContosoDashboard/Pages/Documents.razor` and the relevant notification views

**Checkpoint**: At this point, User Story 3 should be independently functional.

---

## Phase 6: User Story 4 - Keep document workflows aligned with existing project work (Priority: P3)

**Goal**: Documents are visible in the task and dashboard context users already work in.

**Independent Test**: A user adds or views related documents from a project or task, and the dashboard shows recent document activity relevant to that user.

### Implementation for User Story 4

- [ ] T026 [P] [US4] Extend `ContosoDashboard/Services/DashboardService.cs` to include document counts and recent document activity in the dashboard summary
- [ ] T027 [US4] Add the recent documents widget and summary display in `ContosoDashboard/Pages/Index.razor`
- [ ] T028 [US4] Add task and project document attachment context in `ContosoDashboard/Pages/Tasks.razor` and `ContosoDashboard/Pages/ProjectDetails.razor`
- [ ] T029 [US4] Add scan-status visualization and pending-scan messaging in `ContosoDashboard/Pages/Documents.razor` so users understand queued malware processing

**Checkpoint**: At this point, User Stories 1 through 4 should all function independently.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Review cross-cutting quality, security, and validation outcomes after all story work is complete.

- [ ] T030 [P] Review and update the document implementation against `specs/001-document-upload-management/quickstart.md`
- [ ] T031 [P] Perform a security review of direct object access, storage path generation, and access filtering in `ContosoDashboard/Services/DocumentService.cs` and `ContosoDashboard/Services/LocalFileStorageService.cs`
- [ ] T032 [P] Validate the Azure queue scanning design against the offline-first constraints and document the production path in `specs/001-document-upload-management/research.md`
- [ ] T033 Run `dotnet build` and the required manual validation steps for document upload, access denial, and project visibility

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Story Phases (Phase 3-6)**: All depend on the Foundational phase
- **Polish (Phase 7)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational - no dependencies on other stories
- **User Story 2 (P2)**: Can start after Foundational and may integrate with US1 but should remain independently testable
- **User Story 3 (P2)**: Can start after Foundational and may integrate with US1/US2 but should remain independently testable
- **User Story 4 (P3)**: Can start after Foundational and can build on US1/US2/US3 without requiring a full integration block

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- All Foundational tasks marked [P] can run in parallel within Phase 2
- User Story 1 tasks may begin in parallel after Phase 2 completion
- User Story 2 and 3 tasks can proceed in parallel after the core service contracts exist
- Polish tasks marked [P] can be run together after all story work is complete

---

## Parallel Example: User Story 1

```bash
# Run upload UI and metadata work together
Task: "Build the document list and upload UI shell in ContosoDashboard/Pages/Documents.razor"
Task: "Add metadata capture and validation logic in ContosoDashboard/Pages/Documents.razor"

# Run service contracts in parallel once the foundation is ready
Task: "Implement the upload workflow in ContosoDashboard/Services/DocumentService.cs"
Task: "Implement local file persistence in ContosoDashboard/Services/LocalFileStorageService.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 1
4. Validate upload, metadata capture, and access protection independently
5. Stop and evaluate before moving to additional stories

### Incremental Delivery

1. Foundation ready -> Add User Story 1 -> Validate -> Demo
2. Add User Story 2 -> Validate -> Demo
3. Add User Story 3 -> Validate -> Demo
4. Add User Story 4 -> Validate -> Demo
5. Run polish and cross-cutting security review

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1
   - Developer B: User Story 2
   - Developer C: User Story 3
   - Developer D: User Story 4 (dashboard and task integration)
3. The stories can integrate and validate independently before final polish

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] labels map tasks to specific user stories for traceability
- Each user story should be independently completable and testable
- Keep each task concrete and file-directed to avoid ambiguity
- Stop at each checkpoint to validate the story before moving on
- Avoid vague tasks, duplicate work, and cross-story dependencies that break independence
