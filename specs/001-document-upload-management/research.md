# Research: Document Upload and Management

## Decision: Local filesystem storage with a storage abstraction

The project will store uploaded files outside the web root in a local folder such as `AppData/uploads`, with metadata persisted in SQLite and a dedicated storage abstraction layer.

### Rationale

- The repository explicitly requires offline training support and local-only behavior for the MVP.
- Keeping files outside `wwwroot` reduces exposure risk and prevents direct browser access.
- A storage interface allows future migration to Azure Blob Storage without changing business logic or pages.
- The existing application already uses service-layer patterns and role-based authorization, which fits the design.

### Alternatives considered

1. Storing files directly under `wwwroot`
   - Rejected because it exposes upload locations to the browser and makes permission enforcement harder.

2. Direct upload to a cloud service from the start
   - Rejected because the project is explicitly offline-first and designed for training without external service dependencies.

3. Storing the entire content in the database
   - Rejected because it is a poor fit for large files, weakens storage hygiene, and makes migration harder.

## Decision: User and project membership drive document access

Access will be verified in the service layer using the authenticated user, document ownership, project membership, and any explicit share relationships.

### Rationale

- The repository already enforces this pattern for projects and tasks.
- It prevents direct object reference attacks and aligns with the security-by-design constitution.
- It keeps access rules consistent across pages, services, and future API endpoints.

### Alternatives considered

1. Client-side hiding of documents only
   - Rejected because authorization must be enforced server-side.

2. Project manager permission only for all project documents
   - Rejected because it is too restrictive for team collaboration.

## Decision: Metadata model uses integer identifiers and text categories

The feature will follow the repository's established id pattern by using integer IDs for documents, and store category values as text strings such as `Project Documents` and `Personal Files`.

### Rationale

- The stakeholder requirements specify integer `DocumentId` and text category values.
- This matches the existing `UserId`, `ProjectId`, and other EF entities and reduces schema mismatch.
- It keeps the feature simple and consistent with the current training app model.

## Decision: Search and browse rely on EF Core filtering rather than a separate search service

The search and document list screens will filter and sort using the data model and current LINQ patterns already used for projects and dashboard summaries.

### Rationale

- The project already uses simple repository/service patterns and EF Core.
- This approach keeps implementation small and predictable for a training application.
- It allows the feature to meet the required response goals for a small dataset.

## Decision: Queue-based async malware scanning for the Azure production path

The MVP will enforce safe file type validation, size limits, and secure local storage, while the production-ready version will enqueue upload events for asynchronous virus scanning through Azure Queue Storage and an Azure Function-based worker.

### Rationale

- The app is intentionally offline and local-first for the training experience.
- A queue-triggered Azure Function keeps upload latency low and prevents the user experience from hanging on a long scanning task.
- The pattern matches a realistic enterprise design: the web app writes metadata and storage state, then a worker later validates the file, updates status, and quarantines unsafe content.
- The requirement was clarified to keep the feature feasible without compromising the repo's stated design goals.

### Azure workflow design

1. A document is uploaded and validated by the Blazor server flow.
2. The application stores the file safely and writes a document record with a scan status such as `Queued`.
3. A `ScanFileMessage` is sent to Azure Queue Storage.
4. An Azure Function with a Queue Storage trigger reads the message and scans the file using the configured malware engine or external scanner integration.
5. The worker updates the document status to `Approved`, `Quarantined`, or `Failed`, and triggers notifications when needed.
6. The UI can display a status badge such as “Pending scan” until the worker confirms the result.

### Alternatives considered

1. Inline antivirus scanning during upload
   - Rejected because it blocks the request and reduces responsiveness.

2. A local-only background service only
   - Rejected because it is not the intended Azure-grade pattern for production migration.

3. No scan workflow at all
   - Rejected because the feature explicitly requires virus and malware protection at the design level, even if the local training environment defers the live scan dependency.
