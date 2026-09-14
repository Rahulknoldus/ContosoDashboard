# Quickstart: Document Upload and Management Validation

## Prerequisites

- .NET 8 SDK installed
- The ContosoDashboard app is running locally
- A seeded user is available, such as the project manager or employee account
- The project folder has write access for local file uploads

## Setup

1. Start the application:
   ```powershell
   cd ContosoDashboard
   dotnet run
   ```
2. Sign in with an existing mock user account.
3. Confirm the app can access the local storage path for uploads, for example under `AppData/uploads`.

## Validation Scenarios

### 1. Upload a valid document

- Navigate to the document management view.
- Select a supported file such as a PDF or Word document below 25 MB.
- Enter a title, select a category, and optionally assign a project.
- Submit the upload.

**Expected outcome**
- The upload succeeds.
- The document appears in the user’s document list.
- The file is stored outside the web root with a generated safe file path.
- Metadata such as title, category, uploader, size, and timestamp are visible.

### 2. Reject invalid input

- Attempt to upload an unsupported file type or a file larger than 25 MB.

**Expected outcome**
- The system blocks the upload with a clear validation error.
- No file is stored in the local upload directory.
- No document metadata row is created.

### 3. Access enforcement

- Upload a project document as one user.
- Sign out and sign in as a different user who is not in the project or not explicitly shared.

**Expected outcome**
- The second user cannot view, download, or search for the protected document.
- The service denies access through the permission check.

### 4. Search and share flow

- Search by title or tag using a document the user owns or has access to.
- Share the document with a second user.
- Validate the recipient receives the share and can see the file in the shared section.

**Expected outcome**
- Matching results return only accessible documents.
- The recipient sees the shared document after the share action is recorded.
- The audit log records the share action.

## Validation checklist

- [ ] Upload succeeds for a valid PDF or Office file
- [ ] File type validation blocks unsupported documents
- [ ] Size validation blocks files over 25 MB
- [ ] Search returns only accessible documents
- [ ] Project members can view project documents
- [ ] Unauthorized users cannot access restricted documents
- [ ] Shared documents appear in the recipient’s relevant view
- [ ] Audit events are recorded for upload, share, and access actions

## Exit criteria

The feature is ready for implementation review when all validation scenarios complete successfully and the authorization and storage constraints remain aligned with the repository’s offline-first constitution.
