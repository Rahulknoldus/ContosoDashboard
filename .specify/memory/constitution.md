<!--
Sync Impact Report
- Version change: 0.0.0 -> 1.0.0
- Modified principles: none -> I. Security-by-Design, II. Offline-First, Cloud-Ready Architecture, III. Test-First and Verification, IV. User Isolation and Explicit Authorization, V. Simplicity over Cleverness
- Added sections: Project Constraints and Standards; Development Workflow
- Removed sections: none
- Deferred items: Original ratification date for the project constitution is not recorded in the repository; the initial adoption date is set to 2026-09-14 for this governance document.
-->

# ContosoDashboard Constitution

## Core Principles

### I. Security-by-Design
All features must preserve the training application's security model: explicit authentication, role-based authorization, and clear isolation of user data. No page, service, or route may bypass mock identity checks or expose records outside the current user's allowed scope. This principle is non-negotiable because the project is used to teach secure design patterns and because bypasses can create both learning regressions and real security risk.

### II. Offline-First, Cloud-Ready Architecture
The system must remain functional without external cloud services, while keeping infrastructure boundaries explicit. Local development data stores, file handling, and authentication are acceptable for training; cross-cutting abstractions must clarify how Azure or similar services could replace them later. This preserves classroom availability and demonstrates layered architecture without coupling business logic to specific infrastructure.

### III. Test-First and Verification
Changes to behavior must be accompanied by test evidence or a reproducible validation step before merge. New features, security controls, and data-access changes require targeted checks showing the expected outcome and any regressions prevented. This standard ensures that user-facing behavior and security assumptions remain visible and provable instead of fragile or accidental.

### IV. User Isolation and Explicit Authorization
Each user, team member, and project view must obey the least-privilege model. Database queries, service methods, and page access must be scoped by the current identity and allowed roles; direct object access by ID must be validated against membership or permissions. This prevents unauthorized viewing, editing, and cross-user leakage in a multi-user dashboard.

### V. Simplicity over Cleverness
The codebase must favor readable, maintainable, and explainable patterns over abstraction for abstraction's sake. Features should remain small, domain-oriented, and easy to reason about; unnecessary frameworks, hidden magic, or opaque state flows are discouraged. This keeps the repository understandable for learners while making iterative change safer and more predictable.

## Project Constraints and Standards
This repository is a training application, not a production deployment baseline. It must remain suitable for offline classroom use, accept simplified mock implementations, and clearly document any intentional limitations. Core technology standards include ASP.NET Core with Blazor Server, EF Core models, service-layer separation, and explicit authorization checks around protected user data.

Security headers, anti-IDOR checks, and role-aware access control are mandatory for any protected feature. Production migration paths may be documented, but the default behavior of the training app remains local and self-contained. The project must not imply that mock authentication or local-only services are production-ready without additional hardening, identity integration, and operational controls.

## Development Workflow
All changes must follow the same rhythm: clarify scope, implement the smallest valid change, validate behavior, and review for security and maintainability. Feature work must preserve the dashboard's boundaries between Models, Data, Services, and Pages, and must not bypass established authentication or authorization layers. Pull requests and review comments must check for security regressions, user-data isolation, and clarity of intent.

If a feature is intentionally out of scope for the training environment, the limitation must be documented in the relevant specification or README so that the policy remains explicit and reviewable.

## Governance
This Constitution supersedes informal working practices for the repository. Any change that affects access control, project data, user visibility, infrastructure abstraction, or quality gates must document the impact and be reviewed against these principles before merge. Compliance review is expected for each substantial change: security assumptions, role boundaries, and test evidence must be checked.

Amendments require a written change to the constitution, a version bump in accordance with the project versioning policy, and a brief review of the governance impact. Major policy or principle changes require explicit documentation of the reasoning and the expected migration or training impact before adoption. Minor clarifications or wording adjustments may be documented without broad follow-up, but they still require versioning and review.

**Version**: 1.0.0 | **Ratified**: 2026-09-14 | **Last Amended**: 2026-09-14
