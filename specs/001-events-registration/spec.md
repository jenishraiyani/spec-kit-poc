# Feature Specification: Community Events & Registrations

**Feature Branch**: `001-events-registration`  
**Created**: 2026-03-07  
**Status**: Draft  
**Input**: User description: "Create a .NET Core Web API that manages community events and resident registrations. Admin can create events; residents can register; events have limited capacity and waitlists; auto-enroll from waitlist within 12 hours before event when spots open; send email notifications on enrollment."

## User Scenarios & Testing (mandatory)

### User Story 1 - Create Event (Priority: P1)
An Admin creates an event with title, description, start/end times, capacity, and registration window.

**Why this priority**: Foundation for all other workflows.

**Independent Test**: As an Admin, call the Create Event endpoint and verify event persisted and visible in event listing.

**Acceptance Scenarios**:
1. Given Admin authenticated, When Admin submits valid event data, Then event is created and returns 201 with event ID.
2. Given invalid data (end before start), When create called, Then return 400 with validation errors.

---

### User Story 2 - Resident Registration (Priority: P1)
Residents can register for open events until capacity is reached; otherwise they are placed on a waitlist.

**Independent Test**: Register multiple residents until capacity, then one more to confirm waitlist behavior.

**Acceptance Scenarios**:
1. Given event has capacity left, When resident registers, Then registration status = Enrolled and confirmation email sent.
2. Given event is full, When resident registers, Then registration status = Waitlisted and waitlist notification recorded.

---

### User Story 3 - Auto-enroll from Waitlist (Priority: P1)
When spots open within 12 hours before start, the system auto-enrolls residents from waitlist in FIFO order and sends emails.

**Independent Test**: Create event, fill to capacity, add waitlist; free one spot within 12 hours and verify first waitlisted resident becomes Enrolled and receives email.

**Acceptance Scenarios**:
1. Given an open spot appears within 12 hours, When auto-enroll job runs, Then first waitlisted resident is enrolled and emailed.
2. Given multiple openings, When auto-enroll runs, Then fill spots in FIFO order until capacity reached or waitlist exhausted.

---

### Edge Cases (high-priority)
- Concurrent registrations causing race conditions on capacity.
- Duplicate registrations by the same resident for the same event.
- Email delivery failures after enrollment.
- Time zone differences for event start times and 12-hour window.
- Late cancellations and re-opening of seats >12 hours before event (no auto-enroll until within 12 hours window).

## Requirements (mandatory)

### Functional Requirements
- **FR-001**: Admin MUST be able to create, update, and cancel events with metadata (title, description, start/end, location, capacity, registration window).
- **FR-002**: Residents MUST be able to view upcoming events and register for an event.
- **FR-003**: System MUST enforce event capacity; when capacity reached, new registrations are added to a waitlist (ordered FIFO).
- **FR-004**: System MUST support registration statuses: `Enrolled`, `Waitlisted`, `Cancelled`.
- **FR-005**: System MUST send an email notification when a resident becomes `Enrolled` (via direct registration or auto-enroll).
- **FR-006**: System MUST auto-enroll from waitlist for freed spots that occur within the 12-hour window before event start, processing FIFO and sending emails on success.
- **FR-007**: System MUST provide an endpoint for residents to cancel their registration and for admins to cancel registrations.
- **FR-008**: System MUST record audit/log entries for all enrollment, waitlist, cancellation, and auto-enrollment actions.

### Non-functional Requirements
- **NFR-001 (Availability)**: API should be available 99.9% during operating hours.
- **NFR-002 (Performance)**: Typical registration requests should complete within 500ms p95 under expected load (up to 200 req/minpeak for a single event).
- **NFR-003 (Scalability)**: System should handle spike scenarios during high-demand registrations (e.g., burst of 1000 concurrent attempts) with graceful failure and consistent capacity enforcement.
- **NFR-004 (Email delivery)**: Email notifications should be queued reliably; retry policy with exponential backoff for transient failures (configurable retries up to 3 attempts).
- **NFR-005 (Testability)**: Unit tests for business logic and integration tests for registration/workflow must run in CI and be included in PRs.

## API Endpoints
All endpoints require authentication. Admin endpoints require `Admin` role.

- GET /api/events
  - Query: optional filters (from, to, includeCancelled=false)
  - Response: 200 list of events

- POST /api/events
  - Admin only. Body: EventCreateDTO
  - Response: 201 { id }

- GET /api/events/{id}
  - Response: 200 EventDTO or 404

- POST /api/events/{id}/registrations
  - Resident. Body: RegistrationCreateDTO (residentId, contact info optional)
  - Responses:
    - 201 Enrolled (body: RegistrationDTO)
    - 202 Waitlisted (body: RegistrationDTO)
    - 400 validation

- DELETE /api/events/{id}/registrations/{registrationId}
  - Resident or Admin. Cancels registration; Response: 204

- GET /api/events/{id}/registrations
  - Admin only. Response: list of registrations with statuses and timestamps.

- POST /api/events/{id}/cancel
  - Admin only. Cancels event (notify enrolled residents). Response: 200

- GET /api/residents/{id}/registrations
  - Resident. Lists user's registrations and statuses.

Background Jobs / Webhooks
- POST /jobs/auto-enroll
  - Internal or scheduled job that runs frequently (e.g., every minute) to process events whose start time is within 12 hours and have open seats; idempotent and concurrency-safe.

Notification Architecture
- Use an internal reliable queue (e.g., Azure Service Bus, RabbitMQ) to enqueue email notifications; worker sends mail and retries on transient errors.

## Data Model
Use ISO-8601 timestamps with timezone awareness for all date/time fields.

- Event
  - id (GUID)
  - title (string)
  - description (string)
  - location (string)
  - startTime (datetimeoffset)
  - endTime (datetimeoffset)
  - capacity (int)
  - registrationOpen (datetimeoffset)
  - registrationClose (datetimeoffset)
  - status (Scheduled, Cancelled)

- Resident
  - id (GUID)
  - name
  - email (validated)
  - timezone (optional)

- Registration
  - id (GUID)
  - eventId (FK)
  - residentId (FK)
  - status (Enrolled, Waitlisted, Cancelled)
  - createdAt (datetimeoffset)
  - updatedAt (datetimeoffset)
  - position (int) // for waitlist ordering; nullable when Enrolled
  - source (Manual, AutoEnroll)

Indexes & Constraints
- Unique constraint: (eventId, residentId) to prevent duplicate active registrations.
- Index on (eventId, status, position) to efficiently query waitlist order.

## Edge Cases
- Race conditions: Multiple concurrent registrations that would exceed capacity — must use transactional row-level locking or optimistic concurrency (compare-and-swap) to enforce exact capacity.
- Duplicate registration attempts: Deduplicate by unique constraint; return 409 if duplicate active registration exists.
- Cancel and immediately re-register: honor registration window and race rules; if re-registering results in waitlist, return Waitlisted.
- Partial failures: If auto-enroll updates DB but email fails, mark registration as Enrolled and enqueue compensating retry for notification; log failure.
- Time zone edge: Participants in different time zones — always compute 12-hour window relative to event's timezone or stored UTC canonical time; clearly document chosen approach.

## Security Considerations
- Authentication: Use OAuth2 / OpenID Connect or existing identity provider; issue JWT tokens for API access.
- Authorization: Role-based checks (Admin vs Resident). Validate resident identity on registration endpoints.
- Input validation: Strong server-side validation on all endpoints; limit field lengths and sanitize HTML inputs.
- Data protection: Store PII encrypted at rest where required; do not log sensitive fields (PII/email content) in plaintext.
- Rate limiting: Apply per-IP and per-account rate limits to registration endpoint to mitigate abuse.
- Audit trails: Persist audit events for create/update/cancel/auto-enroll actions with actor and timestamps.

## Assumptions
- Resident email addresses are verified by onboarding flow; email addresses are reliable for notifications.
- System clock synchronized (NTP), and UTC is used internally with display conversions for users.
- There is an operational email service with acceptable delivery SLAs.

## Success Criteria
- SC-001: Admins can create events and see them listed immediately (verify via API: create -> GET returns event).
- SC-002: Residents can enroll successfully while capacity available; 95% of single-step enrollments complete within 500ms under normal load.
- SC-003: Waitlist behavior verified: when event full, registrations are marked Waitlisted and ordered FIFO.
- SC-004: Auto-enroll process enrolls waitlisted residents for spots freed within 12 hours in FIFO order; 98% of auto-enroll actions produce an enrollment record and enqueue an email.
- SC-005: Email notifications are retried on transient failure and delivered within configured retry window; failing deliveries are logged and visible to ops.

## Next Steps
- Review API contract and confirm timezone policy for 12-hour window calculations.  
- Generate tasks and tests from user stories (spec -> tasks.md).
