# Implementation Plan: Community Events & Registrations

**Branch**: `001-events-registration` | **Date**: 2026-03-07 | **Spec**: [spec.md](spec.md)

## Summary
Implement an ASP.NET Core Web API that manages community events and resident registrations using Clean Architecture, EF Core with SQL Server, and JWT-based authentication. Key features: admin event management, resident registration with capacity and waitlist, background auto-enroll job (12-hour window), and reliable email notifications via a queue. Design for scalability by keeping API stateless, using a durable relational store for authoritative state, and offloading asynchronous work (emails, auto-enroll processing) to background workers.

## Technical Context
- Language/Version: .NET 8+ (recommend LTS) with C# 12 or later
- Framework: ASP.NET Core Web API
- Architecture: Clean Architecture (API -> Application -> Domain -> Infrastructure)
- ORM: Entity Framework Core (EF Core) with code-first migrations
- Database: SQL Server (primary); plan for read replicas if needed
- Auth: JWT (OAuth2 / OpenID Connect compatible) with role claims (Admin/Resident)
- Background processing: Durable queue/worker (Azure Service Bus / RabbitMQ / Hangfire for orchestrator)
- Observability: Serilog (structured logs), OpenTelemetry (traces + metrics)
- CI: GitHub Actions or Azure DevOps with pipeline for build, tests, SCA, and deployments

## Constitution Check
- Ensure design follows Clean Architecture rules from constitution: dependency flow inward, domain isolation, interfaces at application boundaries. Any deviations require ADR.

## Project Structure (recommended)
```
src/
  ├─ Api/                 # ASP.NET Core Web API (controllers, DTOs, composition root)
  ├─ Application/         # Use cases, commands, queries, interfaces
  ├─ Domain/              # Entities, value objects, domain services, events
  ├─ Infrastructure/      # EF Core DbContext, migrations, email adapter, queue adapter
  └─ Tests/
      ├─ Unit/
      └─ Integration/
```

## Key Components
- Api (composition root)
  - Controllers: thin, map DTOs to application commands/queries
  - Authentication/Authorization middleware (JWT validation, role checks)

- Application Layer
  - Commands/Handlers: CreateEvent, RegisterForEvent, CancelRegistration, AutoEnrollFromWaitlist
  - Queries/Handlers: GetEvents, GetEventDetails, GetRegistrations
  - Interfaces: IEventRepository, IRegistrationRepository, IEmailService, IBackgroundQueue
  - Validation: FluentValidation or custom validators

- Domain Layer
  - Entities: Event, Resident, Registration
  - Domain invariants: capacity enforcement, waitlist ordering logic
  - Domain events: RegistrationEnrolled, RegistrationWaitlisted, RegistrationCancelled

- Infrastructure
  - EF Core DbContext with repositories and transactional patterns
  - Email adapter: enqueue messages to queue and worker consumes
  - Background worker: processes auto-enroll jobs and email sending
  - Migrations folder and scripts

## Concurrency & Capacity Enforcement
- Use database transactions to enforce capacity atomically:
  - Preferred: SELECT ... FOR UPDATE / row-level locking pattern or serializable transaction when adjusting counts.
  - Alternative: optimistic concurrency with a version column on Event and retry loop in application layer (safe but must handle contention).
- Check-and-insert must be atomic: decrement available seats or insert Enrolled and update counts in one transaction. If full, insert Waitlisted row with position.
- Use consistent waitlist position generation (e.g., computed via createdAt and DB-assigned sequence or by position column set in same transaction).

## Auto-enroll Job Design
- Scheduled job (every minute) queries events starting within 12 hours and with open seats and non-empty waitlist.
- For each event, process in a single worker instance using a lease (distributed lock) to avoid parallel workers processing same event.
- For each available slot, within one DB transaction: remove earliest waitlisted registration (FIFO), mark Enrolled, set source=AutoEnroll, persist and enqueue email notification.
- Idempotency: job must be safe to re-run (use a processed flag or check registration status before changing). Use message dedup keys for email sends.

## Email & Notifications
- Enqueue email tasks to durable queue (Service Bus/RabbitMQ). Worker handles sending with retry/backoff.
- All enrollment actions enqueue an EmailNotification record and write audit logs. Failure to deliver does not roll back enrollment.

## Data & Migrations
- EF Core code-first migrations in Infrastructure.Migrations project. Keep migrations small and review in PR.
- Use explicit indexes and constraints: unique(eventId,residentId), index on (eventId,status,position).

## Testing Strategy
- Unit tests: domain logic, command handlers, validation (fast, no DB).
- Integration tests: EF Core in-memory or test SQL Server (Prefer ephemeral SQL Server container) to validate transactions and concurrency scenarios (race tests for capacity).
- Contract tests: API endpoint behaviors (status codes, payloads).
- Load tests: simulate bursting registration attempts to validate capacity enforcement and queuing.

## Observability & Operations
- Structured logging (Serilog) with correlation IDs. Mask PII.
- OpenTelemetry traces for request and background job workflows.
- Metrics: enrollments/sec, waitlist length, email queue length, job success/failure rates.
- Alerts: high error rate, email delivery backlog, job failures, DB deadlocks.

## Security
- JWT tokens with role claims; validate signatures and expirations.
- Role checks at controller or policy level (`[Authorize(Roles = "Admin")]`).
- Input validation and size limits; apply rate limiting (IP/account) to registration endpoints.
- Protect background endpoints and queues with SAS/managed identities.

## CI/CD
- Pipeline steps: restore, build, run analyzers, run unit tests, run integration tests (optional stage), migrate DB in staging, deploy.
- Run SCA (dependency scanning) and fail on critical vulnerabilities.

## Deployment & Scaling
- API: containerized, stateless, autoscale behind load balancer. Use sticky sessions NOT required.
- Database: single primary SQL Server with read replicas; scale up for write-heavy bursts; consider sharding for massive scale.
- Background workers: scale out consumers for email queue; ensure capacity for auto-enroll job via distributed locking.

## Tasks (high level)
- T1: Create solution and projects (Api, Application, Domain, Infrastructure)
- T2: Implement Domain models + unit tests
- T3: Implement EF Core schemas + migrations
- T4: Implement repository interfaces and concrete EF implementations
- T5: Implement API controllers and DTOs
- T6: Implement JWT auth + role policies
- T7: Implement registration flow with transactional capacity enforcement
- T8: Implement background worker and auto-enroll job with distributed locking
- T9: Implement email queue adapter and worker
- T10: Add logging, metrics, and OpenTelemetry instrumentation
- T11: Write integration and load tests for concurrency scenarios
- T12: Configure CI pipeline and deployment manifests

## Rollout & Validation
- Deploy to staging, run integration and load tests; verify auto-enroll behavior with time-shifted test events.
- Verify email queue and retries in staging.
- Monitor metrics and alerts post-deploy; run smoke tests for registration and admin flows.

## Open Questions / Risks
- Choice of distributed lock: DB-based (sp_getapplock) vs Redis/Tokens vs Service Bus locks — select based on infra.
- Email provider SLA and scaling — ensure worker backpressure and dead-letter handling.

## Appendices
- See `spec.md` for detailed API and data model. Align DTOs and OpenAPI during implementation.

---

**Structure Decision**: Single-repo, multi-project structure per Clean Architecture as shown above.

**Complexity Tracking**: No constitution violations expected; concurrency enforcement is critical and must be explicitly validated in integration tests.
