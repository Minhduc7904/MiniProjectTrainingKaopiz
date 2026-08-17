---
name: qa.docs.writing
description: Use when creating or updating technical documentation, specs, or READMEs to ensure clarity and professional formatting.
when_to_use: Activate whenever a feature or bugfix implementation is complete (GREEN) or when the user explicitly requests documentation. Mandatory before the Deliver phase.
license: MIT
---

# QA — Technical documentation writing

**Skill id:** `qa.docs.writing`

Documentation is how you communicate with the future. Good docs eliminate guesswork.

## Mandatory Structure

1.  **Purpose**: Why does this exist? What problem does it solve?
2.  **Architecture**: Use **Mermaid diagrams** for data flow or component relationships.
3.  **Usage**: Provide **runnable examples** or clear API signatures.
4.  **Troubleshooting**: Document common failure modes and their fixes.

## Iron Law — No Code without Docs

If a code change affects external behavior or configuration, the documentation MUST be updated in the same task. A PR without updated docs is a quality failure.

## Excuse vs. Reality

| Excuse | Reality |
| :--- | :--- |
| "The code is self-documenting." | Code shows *how*, documentation explains *why* and *how to use*. |
| "I'll add the Mermaid diagram in the next PR." | Diagrams are critical for architecture review. Add them now. |
| "This is just an internal tool, no docs needed." | Internal tools are where most time is lost to legacy confusion. |

## Formatting Standards

- Use **GitHub Alerts** (`> [!NOTE]`, `> [!IMPORTANT]`) for critical info.
- Use **Tables** for configuration or parameter references.
- Use **Code Blocks** with language identifiers for all snippets.

## See also

- `phase.deliver.final` — documentation is a prerequisite for final delivery.
- `phase.spec.brainstorm` — documenting the initial design before implementation.

---
**Summary:** Professionalism is measured by the quality of your handoff.
