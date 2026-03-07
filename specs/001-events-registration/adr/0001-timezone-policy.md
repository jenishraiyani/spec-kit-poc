# ADR 0001 — Timezone policy for auto-enroll 12-hour window

## Status
Accepted

## Context
The system must auto-enroll residents from the waitlist when seats free up within a 12-hour window before event start. The team must decide which timezone to use to compute the 12-hour window to ensure deterministic and expected behavior across participants in different timezones.

## Decision
Compute the 12-hour auto-enroll window relative to the event's timezone. Store all timestamps in the database as UTC (`datetimeoffset`), but persist the event's declared timezone (IANA or Windows timezone id) on the Event record. Convert event start time to the event's timezone when evaluating the 12-hour window.

## Rationale
- Aligns the enrollment window with the organizer's/local expectations for the event (e.g., community center local time).
- Avoids surprising behavior for residents in other timezones where the event's local time is the single source of truth.
- Storing UTC timestamps preserves ordering and simplifies comparisons across systems, while the stored timezone provides the mapping for local calculations.

## Consequences
- Implementation must include timezone metadata on `Event` (e.g., `eventTimezone` property).
- Background jobs (auto-enroll) must convert stored UTC start time into the event's local time using the event's timezone and compute the 12-hour threshold accordingly.
- Tests must include scenarios with events and residents across DST transitions and different timezones.
- API documentation must indicate that the 12-hour window is evaluated in the event's timezone.

## Implementation Notes
- Add `Timezone` (string) field to `Event` entity and DB schema; default to UTC when unspecified.
- Use `TimeZoneInfo` (Windows) or NodaTime (recommended for cross-platform, IANA) for conversions. Prefer NodaTime for correct IANA support and DST handling.
- Auto-enroll job flow:
  1. For each candidate event, load `startTimeUtc` and `eventTimezone`.
  2. Convert `startTimeUtc` to event local time.
  3. Compute threshold = localStartTime - 12 hours; convert threshold back to UTC for DB queries.
  4. Query events with `startTimeUtc >= nowUtc` and `startTimeUtc <= thresholdUtc` and open seats.
- Add unit and integration tests that simulate DST boundaries and timezone offsets.

## Migration Plan
- Add migration to add `Timezone` column to `Events` table with default `UTC`.
- Backfill existing events with `UTC` unless a specific timezone is known.

**Date**: 2026-03-07
**Authors**: Implementer
