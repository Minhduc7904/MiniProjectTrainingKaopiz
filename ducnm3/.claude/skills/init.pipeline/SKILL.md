---
name: init.pipeline
description: Use when designing the CI/CD pipeline (GitHub Actions, GitLab CI) to automate testing, linting, and deployment.
when_to_use: Activate during project setup. Mandatory before merging the first feature branch.
license: MIT
---

# Init — Pipeline (CI/CD)

**Skill id:** `init.pipeline`

The pipeline is the "Guardian of Quality". It must be fast, reliable, and unforgiving.

## Iron Law — No Merge without CI Green

The main branch must be protected. No code enters without passing the automated pipeline. No exceptions.

## Pipeline Stages

1.  **Lint/Format**: Catch stylistic and low-level issues first (fastest).
2.  **Test**: Run unit and integration tests (parallelized where possible).
3.  **Build**: Verify the production artifact can be generated.
4.  **Security**: Scan for secrets and vulnerable dependencies.

## Excuse vs. Reality

| Excuse | Reality |
| :--- | :--- |
| "The CI is too slow, I'll just push directly." | Pushing directly is how production breaks. Optimize the CI instead of bypassing it. |
| "I'll setup the deployment part later." | Deployment automation is critical for minimizing "Friday Afternoon" risk. |

## See also

- `phase.verify.compliance` — the rules the pipeline enforces.
- `release.prep.checklist` — the final human check before the pipeline ships.

---
**Summary:** Automation is the only way to scale quality.
