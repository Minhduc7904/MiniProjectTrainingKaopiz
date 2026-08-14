# Git Rules

- Keep all changes inside `ducnm3/` unless the user explicitly authorizes another path.
- `ducnm3` is the integration branch. Merge ticket pull requests into `ducnm3`.
- From Day 3 onward, use branch `feature/{backlog-key}` (for example
  `feature/BLD-124_4`), based on `ducnm3`. Do not use `feature/ducnm3_<short-description>`.
- Commit message phải viết bằng tiếng Việt, ngắn gọn nhưng nêu rõ hành động và
  phạm vi thay đổi chính. Dùng tiêu đề task backlog khi tiêu đề đó đã rõ nghĩa;
  không dùng prefix như `feat:` hoặc message chung chung như `update`, `fix`,
  `wip`.
- Do not push **code** when the matching `docs/plan/` task has Ticket `Chưa tạo`
  or no Jira key, even if the user asks to push. Local commits are allowed.
- Documentation-only markdown (`docs/**/*.md`, `AGENTS.md`, `rules/*.md`,
  `.agents/**/*.md`) may be committed and pushed directly to `ducnm3` without a
  ticket branch or pull request. Mixed diffs that include `backend/`,
  `frontend/`, `tests/`, or `scripts/` application code still require the ticket
  branch.
- Keep each change focused; do not include unrelated cleanup.
- Do not commit or push unless the user explicitly asks.
- When the user asks, create a pull request targeting `ducnm3`. Title = backlog
  task title. Body must match the Vietnamese template in `docs/guide/GIT_GUIDE.md`.
- Review a pull request only when the user asks.
