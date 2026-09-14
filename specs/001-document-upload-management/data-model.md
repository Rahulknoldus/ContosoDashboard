# Data Model: Document Upload and Management

## Core Entities

### Document

Represents a stored file and its user-visible metadata.

**Fields**
- DocumentId: integer, primary key
- Title: string, required, max length 255
- Description: string, optional, max length 2000
- Category: string, required, one of the allowed categories
- ProjectId: integer, optional, references the related project
- TaskId: integer, optional, references the related task
- UploadedByUserId: integer, required, references the owner/user
- FileName: string, required, safe generated name without user-controlled path segments
- StoredFilePath: string, required, relative storage path used by file service
- FileSizeBytes: long, required
- MimeType: string, required, max length 255
- UploadedAtUtc: datetime, required
- UpdatedAtUtc: datetime, required
- IsDeleted: bool, default false

**Validation rules**
- Title must be present.
- Category must be one of: Project Documents, Team Resources, Personal Files, Reports, Presentations, Other.
- File size must be <= 25 MB.
- File extension must be on the approved allowed list.
- File path must be generated before database insertion and must not come from user input.

### DocumentShare

Tracks explicit access granted beyond project membership.

**Fields**
- DocumentShareId: integer, primary key
- DocumentId: integer, required
- SharedWithUserId: integer, required
- SharedByUserId: integer, required
- SharedAtUtc: datetime, required
- Message: string, optional
- IsActive: bool, default true

**Relationships**
- Many-to-one with Document
- Many-to-one with User

### DocumentAccessLog

Records document access and lifecycle events for audit usage.

**Fields**
- LogId: integer, primary key
- DocumentId: integer, required
- UserId: integer, required
- ActionType: string, required (Upload, Download, Preview, Delete, Share, Replace)
- ActionAtUtc: datetime, required
- Details: string, optional, for context

**Relationships**
- Many-to-one with Document
- Many-to-one with User

## Relationships

- User: one-to-many with Document (uploaded by user)
- User: one-to-many with DocumentShare (shared by or shared with user)
- User: one-to-many with DocumentAccessLog
- Project: one-to-many with Document
- TaskItem: one-to-many with Document
- Document: one-to-many with DocumentShare
- Document: one-to-many with DocumentAccessLog

## State transitions

- Draft upload: metadata collected, file validation pending
- Stored: file saved to storage and metadata persisted
- Shared: explicit access granted to another user
- Updated: file replaced or metadata changed
- Deleted: soft-delete or permanent removal after confirmation

## Notes

The database should remain simple and aligned with the current training app patterns. Document IDs are integers, category is stored as a text value, and file paths are relative and generated from GUID-backed filenames to remain secure and portable.
