# Git Workflow Guide

## Branch conventions

- `ducnm3` is the default integration branch.
- Use a feature branch only for a focused change that will be reviewed through a pull request.
- Feature branches follow `feature/ducnm3_<short-description>`, for example `feature/ducnm3_health-api-contracts`.

## Start a feature

```bash
git switch ducnm3
git pull --ff-only origin ducnm3
git switch -c feature/ducnm3_<short-description>
```

Keep each branch scoped to one purpose. Do not include generated artifacts, credentials, `.env`, or unrelated cleanup.

## Daily workflow

```bash
git status --short
git diff --check
dotnet build backend/Lms.sln -m:1
git add <changed-files>
git commit -m "add database-backed service health checks"
```

Use an imperative commit subject that describes the outcome. Before committing, verify tests or the relevant local runtime workflow.

## Open a pull request

```bash
git push -u origin feature/ducnm3_<short-description>
gh pr create --base ducnm3 --title "<concise title>" --body "<summary and test plan>"
```

The PR should state:

- the behavioral or architectural change;
- important configuration or migration implications;
- exact verification commands and results;
- follow-up work that is intentionally excluded.

## Update a feature branch

When `ducnm3` has new commits, update the feature branch with a non-destructive merge:

```bash
git fetch origin
git merge origin/ducnm3
```

Resolve conflicts locally, rebuild/test, then commit the merge result. Do not force-push shared branches.

## Finish

After the pull request is merged, switch back to `ducnm3` and remove the merged local branch:

```bash
git switch ducnm3
git pull --ff-only origin ducnm3
git branch -d feature/ducnm3_<short-description>
```
