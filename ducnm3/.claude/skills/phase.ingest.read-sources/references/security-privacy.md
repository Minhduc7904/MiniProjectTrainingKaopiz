# Security & privacy — optional prompts (ingest)

Summarize in **`.vibe/research/<task-id>.md`** — **no** full secrets, tokens, or PII dumps.

| Topic | Questions |
| ----- | --------- |
| **AuthZ / AuthN** | Who can do this? Role changes? Service-to-service auth? |
| **Data classification** | PII, PCI, health data? Retention / deletion? |
| **Secrets** | New env vars or vault paths — named only, not values. |
| **Audit** | Compliance logging required for this change? |
| **Threats** | Injection, SSRF, path traversal relevant to touched surfaces? |
