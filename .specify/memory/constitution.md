<!--
Sync Impact Report
- Version change: none → 1.0.0
- Modified principles: (new) Clean Architecture; SOLID & Maintainable Code; High Code Quality; Testing: Unit & Contract-First; Performance and Scalability; Security & Compliance; API Design, Logging & Observability
- Added sections: Security & Compliance; API Design, Logging & Observability
- Removed sections: none
- Templates checked: ✅ .specify/templates/plan-template.md (aligned)
                   ✅ .specify/templates/spec-template.md (aligned)
                   ✅ .specify/templates/tasks-template.md (aligned)
                   ✅ .specify/templates/checklist-template.md (aligned)
- Follow-up TODOs: none
-->

# .NET Core Engineering Constitution

## Core Principles

### I. Clean Architecture (NON-NEGOTIABLE)
All production code MUST follow Clean Architecture layering: UI/API → Application/Services → Domain (entities, domain services) → Infrastructure. Dependencies flow inward toward the Domain. Implement ports-and-adapters (hexagonal) where services or external systems are consumed. Frameworks and ORMs belong to the Infrastructure layer and MUST NOT leak domain concepts.

Rules:
- Separate responsibilities into projects/assemblies that map to layers (e.g., Api, Application, Domain, Infrastructure).
- Use interfaces and abstractions to invert dependencies; inject implementations at composition root.
- Avoid circular dependencies and cross-layer references.
- Small, cohesive assemblies preferred over a single monolithic project.

Rationale: Maintaining strict layering reduces coupling, improves testability, and makes architectural trade-offs explicit and reviewable.

### II. SOLID & Maintainable Code
Code MUST adhere to SOLID principles and idioms that promote readability and extensibility.

Rules:
- Single Responsibility: classes and modules should have one reason to change.
- Open/Closed: prefer extension via abstractions rather than modification of existing code.
- Liskov Substitution: derived types MUST be substitutable for their base types.
- Interface Segregation: prefer narrow, role-focused interfaces.
- Dependency Inversion: depend on abstractions, not concretions.
- Favor composition over inheritance; prefer small cohesive types.

Rationale: SOLID reduces regression risk and makes code easier to reason about during reviews and refactors.

### III. High Code Quality & Standards
All code MUST meet the project's quality standards enforced by CI and developer tooling.

Rules:
- Adopt and enforce a shared style via `.editorconfig`, Roslyn analyzers, and formatting tools (e.g., `dotnet format`).
- Enable nullable reference types and fix warnings prior to merging.
- Fail builds on critical analyzer errors; surface warnings as part of PR review.
- Keep PRs small and focused (prefer <300 changed lines; justify larger PRs with an ADR).
- Public APIs MUST be documented with XML comments and included in API docs via OpenAPI where applicable.

Rationale: Consistent style and automated checks reduce review overhead and surface defects earlier.

### IV. Testing: Unit & Contract-First
Testing is mandatory. Tests are first-class artifacts and part of the definition of done.

Rules:
- Follow a testing pyramid: unit tests (fast, isolated) >> integration tests (db, infra) >> end-to-end/contract tests.
- Unit tests MUST cover business logic; use xUnit (recommended) or equivalent. Use mocking for external dependencies.
- Contract/integration tests REQUIRED for public APIs and cross-service contracts; run in CI.
- Establish minimum coverage goals for critical domain code (suggested baseline: 80% for business logic). Coverage exceptions must be documented and approved.
- Tests MUST be included in the same PR as code changes; tests must fail before implementation (TDD encouraged but not mandatory).

Rationale: Reliable automated tests prevent regressions and enable safer refactoring and faster delivery.

### V. Performance and Scalability
Systems MUST be designed and measured for their expected load and resource constraints.

Rules:
- Define performance targets (latency, throughput) in the plan and validate with benchmarks and load tests.
- Measure before optimizing; use BenchmarkDotNet for hot-path microbenchmarks.
- Prefer asynchronous I/O (`async`/`await`), pooling, and streaming for high-throughput paths.
- Avoid excessive allocations and boxing in hot paths; prefer value types and spans where applicable.
- Cache thoughtfully with clear invalidation rules; document cache consistency and TTLs.

Rationale: Targeted measurement plus disciplined optimization prevents premature optimization and ensures predictable operational characteristics.

## Security & Compliance
Security is integral to design, development, and operations.

Rules:
- Follow OWASP Top Ten guidance for web-facing services. Perform threat modeling for new public endpoints.
- Secrets MUST NOT be checked into source control. Use secure secret stores (Azure Key Vault, GitHub Actions secrets) and environment-based configuration.
- Validate and sanitize all inputs; enforce strong typing and avoid stringly-typed logic for auth/permissions.
- Use proven crypto libraries and follow platform guidance; do not invent custom encryption.
- Apply dependency scanning and patch vulnerable dependencies promptly (SCA in CI).
- Log security-relevant events (auth failures, permission denials) while avoiding sensitive data in logs.

Rationale: Embedding security practices prevents common vulnerabilities and supports compliance obligations.

## API Design, Logging & Observability
APIs and telemetry MUST be consistent, discoverable, and operable.

API Design Rules:
- Expose APIs with explicit contracts (OpenAPI/Swagger); changes to contracts MUST follow versioning rules.
- Use consistent URI design, HTTP verbs, status codes, and pagination patterns. Document error payloads and codes.
- Version public APIs (path or header-based); maintain backward compatibility where possible. Breaking changes require an ADR and migration plan.
- Idempotency: design write endpoints to support safe retries where appropriate.

Logging & Observability Rules:
- Use structured logging (Serilog recommended) with consistent property names. Include correlation IDs for request tracing.
- Emit metrics for business KPIs and technical health (latency, error rate, queue lengths). Instrument with OpenTelemetry and export to chosen backend.
- Trace distributed requests end-to-end; capture spans for critical operations.
- Redact or avoid logging PII or secrets. Document log retention and access controls.

Rationale: Good API design and observability reduce debugging time and make operational behavior visible.

## [Development Workflow & Quality Gates]
All engineering activity shall follow defined workflow and gates.

Rules:
- Every change requires a pull request with a clear description, linked issue/spec, and test coverage.
- PR checklist (enforced by reviewers/CI):
  - Architecture: Does the change respect layering and Clean Architecture?
  - SOLID: Are abstractions appropriate? Any new interfaces justified?
  - Tests: Are unit and integration tests included and passing?
  - Security: Any new secrets, sensitive data, or public endpoints reviewed?
  - Performance: Any hot-path changes validated or benchmarked?
  - API: OpenAPI updated for public-facing changes; versioning considered.
  - Observability: Logs/metrics/traces added for new behavior.
  - Linting/Formatting: CI analyzers and formatters pass.
- Complexity exceptions or technical debt incursions MUST be recorded as an ADR and include a remediation plan and timeline.

Rationale: Standardized workflow reduces risk and ensures consistency across contributions.

## Governance
This constitution defines non-negotiable engineering principles. It guides technical decisions, reviews, and implementations.

Decision Making & Exceptions:
- Minor deviations (local refactors, experimental prototypes) may be accepted by the author and a single reviewer, but MUST be labeled and time-boxed.
- Significant deviations (new cross-cutting frameworks, breaking API changes, architectural shifts) require an ADR and majority approval from core maintainers.
- All ADRs SHOULD include: context, options considered, consequences, and rollback/migration plan.

Code Reviews & Compliance:
- Reviewers are responsible for verifying adherence to this Constitution during PR review. CI checks must block merges on failing gates.
- Non-compliant changes must be accompanied by a documented justification in the PR and an ADR when the change affects architecture or policy.

Versioning & Amendments:
- Versioning follows MAJOR.MINOR.PATCH semantics.
  - MAJOR: Removing or redefining principles or other breaking governance changes.
  - MINOR: Adding new principles or materially expanding guidance.
  - PATCH: Clarifications, wording fixes, and non-substantive edits.
- Ratification requires approval by the project's core maintainers. Amendments MUST be recorded with an ADR and the constitution's `Last Amended` date updated.

Compliance & Review Cadence:
- Quarterly reviews are recommended to ensure the constitution remains aligned with the team's needs and technology changes.

**Version**: 1.0.0 | **Ratified**: 2026-03-07 | **Last Amended**: 2026-03-07
