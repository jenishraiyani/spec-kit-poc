# Events API Documentation Skill

Name: events-docs-skill

Purpose:
- Help Copilot generate concise API documentation for Events and Registrations by scanning the project's ASP.NET Core controllers.

When to use:
- Use when you need an up-to-date reference of endpoints implemented in src/Api/Controllers (for example, to create API docs, README sections, or to include examples in PRs).

How the skill helps Copilot:
- Provides a prompt template and a lightweight parsing script that extracts route attributes and HTTP verbs from controller source files, producing JSON and Markdown output that Copilot can use as the basis for generated documentation.

Inputs expected:
- The repository root contains `src/Api/Controllers/*.cs` files.
- Optionally provide a target output path for generated docs (defaults to `.github/skills/events-docs/output/`).

Outputs produced:
- `events_api.json` — structured JSON describing controllers and endpoints.
- `events_api.md` — a simple Markdown reference of endpoints.

Prompt templates (examples):

1) Generate full API reference

"""
Using the parsed controller output at `.github/skills/events-docs/output/events_api.md`, produce a developer-friendly Events API reference section suitable for README.md. Include example requests and success responses.
"""

2) Produce OpenAPI-like summary

"""
Convert the JSON at `.github/skills/events-docs/output/events_api.json` into an OpenAPI-style summary (paths with verbs, brief descriptions). Keep entries short (1-2 lines each).
"""

Examples (how a developer or Copilot prompt can call the skill):

- Run the parse script: `python .github/skills/events-docs/parse_controllers.py`
- Then ask Copilot: "Use the generated `.github/skills/events-docs/output/events_api.md` to write a README section describing the Events endpoints with example curl commands."

Notes:
- This skill is intentionally simple and heuristic-based — review its output for edge cases (complex route building, parameters from attributes, or custom routing logic).
- If the project uses API versioning or route conventions, run this tool after building or provide the explicit routing policy to Copilot.
