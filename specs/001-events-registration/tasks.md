---
description: "Task list for Community Events & Registrations"
---

# Tasks: Community Events & Registrations

**Input**: `spec.md`, `plan.md`  
**Prerequisites**: Clean Architecture plan, EF Core, SQL Server, JWT auth

## Phase 1: Setup (Shared Infrastructure)

- [ ] T001 Create repository solution and projects: src/Api, src/Application, src/Domain, src/Infrastructure
- [ ] T002 Initialize `dotnet` solution and projects (target .NET 8) in repository root
- [ ] T003 [P] Add CI pipeline skeleton (`.github/workflows/ci.yml`) with build, analyzers, unit tests
- [ ] T004 [P] Add `.editorconfig`, `dotnet format` config, and Roslyn analyzer rules in repo
- [ ] T005 [P] Add Docker Compose for local SQL Server and a worker (docker/docker-compose.yml)
- [ ] T006 [P] Add README with setup steps and env var conventions (README.md)

---

## Phase 2: Foundational (Blocking Prerequisites)

- [ ] T007 Implement Domain entities: `src/Domain/Entities/Event.cs`, `src/Domain/Entities/Resident.cs`, `src/Domain/Entities/Registration.cs`
- [ ] T008 [P] Define repository interfaces in `src/Application/Interfaces/IEventRepository.cs` and `IRegistrationRepository.cs`
- [ ] T009 Implement EF Core DbContext and mappings in `src/Infrastructure/Persistence/AppDbContext.cs` and initial migration in `src/Infrastructure/Migrations`
- [ ] T010 [P] Implement repository EF adapters in `src/Infrastructure/Repositories/EventRepository.cs` and `RegistrationRepository.cs`
- [ ] T011 Implement JWT authentication and role policies in `src/Api/Startup` (or Program.cs) and `src/Api/Auth/` (JWT configuration)
- [ ] T012 [P] Add IEmailService and queue adapter interfaces in `src/Application/Interfaces/IEmailService.cs` and `src/Application/Interfaces/IBackgroundQueue.cs`
- [ ] T013 Implement email queue adapter stub (e.g., `src/Infrastructure/Email/ServiceBusEmailSender.cs`) and local worker scaffold `src/Infrastructure/Workers/EmailWorker.cs`
- [ ] T014 [P] Add logging and telemetry setup: Serilog config in `src/Api/Logging` and OpenTelemetry instrumentation
- [ ] T015 Add API contract and OpenAPI generation in `src/Api` (Swashbuckle) and initial swagger docs
- [ ] T016 [P] Add test projects structure: `tests/Unit`, `tests/Integration` and test runner config
- [ ] T017 Implement database seed scripts for local development `src/Infrastructure/Seed/InitialData.cs`

---

## Phase 3: User Story 1 - Create Event (Priority: P1)

**Goal**: Admins can create, update, and cancel events with validation

**Independent Test**: API tests for create -> GET

- [ ] T018 [US1] [P] Create DTOs and validators: `src/Api/Dto/EventCreateDto.cs`, `src/Application/Validators/EventCreateValidator.cs`
- [ ] T019 [US1] Implement `CreateEvent` command and handler: `src/Application/Commands/CreateEvent/CreateEventCommand.cs`
- [ ] T020 [US1] Implement `GetEvents` query and handler: `src/Application/Queries/GetEvents/GetEventsQuery.cs`
- [ ] T021 [US1] Implement API controller endpoints: `src/Api/Controllers/EventsController.cs` (POST /api/events, GET /api/events/{id})
- [ ] T022 [US1] Add unit tests for domain invariants and command handler: `tests/Unit/CreateEventTests.cs`
- [ ] T023 [US1] Add integration test for create + read (SQL Server test instance): `tests/Integration/CreateEventIntegrationTests.cs`

---

## Phase 4: User Story 2 - Resident Registration (Priority: P1)

**Goal**: Residents can register; capacity enforced; waitlist created when full

**Independent Test**: Registration flow test that verifies Enrolled vs Waitlisted

- [ ] T024 [US2] [P] Create Registration DTOs and validators: `src/Api/Dto/RegistrationCreateDto.cs`, `src/Application/Validators/RegistrationCreateValidator.cs`
- [ ] T025 [US2] Implement `RegisterForEvent` command and handler with transactional capacity enforcement: `src/Application/Commands/RegisterForEvent/RegisterForEventCommand.cs`
- [ ] T026 [US2] Implement repository method to atomically check capacity and insert enrollment/waitlist in `src/Infrastructure/Repositories/RegistrationRepository.cs`
- [ ] T027 [US2] Implement API controller endpoint: `src/Api/Controllers/RegistrationsController.cs` (POST /api/events/{id}/registrations)
- [ ] T028 [US2] Add unit tests for registration business logic and concurrency edge cases: `tests/Unit/RegisterForEventTests.cs`
- [ ] T029 [US2] Add integration tests that simulate concurrent registration attempts to verify capacity enforcement: `tests/Integration/ConcurrentRegistrationTests.cs`
- [ ] T030 [US2] Implement email enqueue on successful enrollment (call IEmailService) in `src/Application/Services/NotificationService.cs`

---

## Phase 5: User Story 3 - Auto-enroll from Waitlist (Priority: P1)

**Goal**: Auto-enroll waitlisted residents into freed spots within 12-hour window and send emails

**Independent Test**: Auto-enroll job test including idempotency and email enqueue

- [ ] T031 [US3] Implement background job scheduler and worker: `src/Infrastructure/Workers/AutoEnrollWorker.cs`
- [ ] T032 [US3] Implement auto-enroll use case handler: `src/Application/Jobs/AutoEnroll/AutoEnrollHandler.cs`
- [ ] T033 [US3] Add distributed lock/lease logic to avoid concurrent processing of the same event: `src/Infrastructure/Locking/LeaseManager.cs`
- [ ] T034 [US3] Add integration tests for auto-enroll behavior (time-shifted event start): `tests/Integration/AutoEnrollIntegrationTests.cs`
- [ ] T035 [US3] Ensure email enqueue and retry on failures; wire worker to process notification queue: `src/Infrastructure/Workers/EmailWorker.cs`

---

## Phase 6: Polish & Cross-Cutting Concerns

- [ ] T036 [P] Add OpenAPI contract completion and example payloads (`src/Api/Swagger`)
- [ ] T037 [P] Add end-to-end smoke tests for Admin + Resident flows: `tests/Integration/SmokeTests.cs`
- [ ] T038 [P] Add load-testing scripts and CI job (e.g., k6) to simulate registration spikes
- [ ] T039 [P] Document audit and logging policy in `docs/observability.md`
- [ ] T040 [ ] Security review and threat model document `docs/security/threat-model.md`
- [ ] T041 [P] Final code cleanup, enforce analyzers, and fix warnings before release

---

## Remediation & Governance Tasks (from spec analysis)

- [ ] T042 Implement `UpdateEvent` command/handler and `PUT /api/events/{id}` controller + unit and integration tests
- [ ] T043 Implement `CancelEvent` command/handler and `POST /api/events/{id}/cancel` implementation with notification to enrolled residents + tests
- [ ] T044 Implement `CancelRegistration` (DELETE /api/events/{id}/registrations/{registrationId}) handler with resident/admin authorization checks + unit and integration tests
- [X] T044 Implement `CancelRegistration` (DELETE /api/events/{id}/registrations/{registrationId}) handler with resident/admin authorization checks + unit and integration tests
- [ ] T045 Implement audit persistence: add `Audit` table, write hooks in application handlers for enrollment/waitlist/cancel/auto-enroll, and tests to validate audit entries
- [ ] T046 Add explicit DB migration tasks to create/verify constraints and indexes: unique(eventId,residentId) and index(eventId,status,position) and test migration behavior in integration tests
- [ ] T047 Record Timezone Decision ADR (recommended: event's timezone) and implement timezone-normalized storage and tests for the 12-hour window behavior
- [ ] T048 Add integration tests for email failure scenarios: simulate delivery failures, verify enrollment persists, notification retries, and dead-letter handling
- [ ] T049 Decide & implement distributed lock/lease mechanism for auto-enroll (DB sp_getapplock or Redis/Service) and add integration tests to validate single-processor semantics

---

## Dependencies & Execution Order

- Phase 1 (Setup): start immediately
- Phase 2 (Foundational): blocks all user stories
- Phases 3-5 (User Stories): depend on Phase 2; can run in parallel across different team members
- Phase 6 (Polish): after user stories complete

## Parallel Execution Examples

- `T003`, `T004`, `T005`, `T006` (Setup tasks) can run in parallel.  
- `T008`, `T010`, `T012`, `T014`, `T016` (Foundational adapters, logging, tests) can run in parallel.  
- Within a user story, creating DTOs/validators and unit tests (e.g., `T018`, `T022`) are parallelizable.

## Implementation Strategy

- MVP scope: Implement Phase 1 + Phase 2 + Phase 3 + Phase 4 (Create Event + Resident Registration with waitlist and emails).  
- Deliver Auto-enroll (Phase 5) as the next increment after basic registration is stable and tested.  
- Use feature toggles for auto-enroll and email delivery during rollout.

## Notes

- All tasks must include tests where applicable.  
- Use transactional patterns in `T025`/`T026` to ensure capacity enforcement.  
