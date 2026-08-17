#!/usr/bin/env npx tsx
/**
 * Claude Code / Cursor hook: log skill usage into the active task's step folder.
 *
 * Fires on three events:
 *   UserPromptSubmit  — user types /skillname in prompt
 *   PreToolUse/Skill  — model invokes the Skill tool
 *   PreToolUse/Read   — model reads a SKILL.md under .cursor/skills or .claude/skills
 *
 * Always allows (never blocks). When a DevKit session is active (.vibe/active.json)
 * and a step is running (.vibe/sessions/<seg>/state.json), it appends the skill entry
 * to a per-step aggregated JSON file:
 *
 *   .vibe/logs/<author>/<task>/steps/<NN>-<flat_id>/skill-reads.json
 *
 * One JSON file per step — never duplicated across steps, no project-wide flat log.
 *
 * Supported platforms detected via env vars:
 *   CURSOR_PROJECT_DIR   → cursor
 *   CLAUDE_PROJECT_DIR   → claude
 *   ANTIGRAVITY_ROOT     → antigravity
 *   (fallback)           → unknown
 */

import fs from "node:fs";
import path from "node:path";
import readline from "node:readline";
import { execFileSync } from "node:child_process";

type Payload = Record<string, unknown>;

// ---------------------------------------------------------------------------
// Platform & path resolution
// ---------------------------------------------------------------------------

function detectPlatform(): string {
  if (process.env.CURSOR_PROJECT_DIR) return "cursor";
  if (process.env.CLAUDE_PROJECT_DIR) return "claude";
  if (process.env.ANTIGRAVITY_ROOT) return "antigravity";
  return "unknown";
}

function projectRoot(): string {
  return (
    process.env.CURSOR_PROJECT_DIR ??
    process.env.CLAUDE_PROJECT_DIR ??
    process.env.ANTIGRAVITY_ROOT ??
    process.cwd()
  );
}

function vibeLogsDir(root: string): string {
  return path.join(root, ".vibe", "logs");
}

// ---------------------------------------------------------------------------
// Git identity — matches gitEmailPathSegment() in session-log.ts:
//   user.name (local → global) first, then user.email with @ → -at-
// ---------------------------------------------------------------------------

function tryGit(root: string, scope: string, key: string): string {
  try {
    return execFileSync("git", ["config", scope, key], {
      cwd: root,
      encoding: "utf8",
      stdio: ["ignore", "pipe", "ignore"],
      timeout: 2000,
    }).trim();
  } catch {
    return "";
  }
}

function gitAuthorSlug(root: string): string {
  const name =
    tryGit(root, "--local", "user.name") || tryGit(root, "--global", "user.name");
  if (name) return sanitizeSeg(name);
  const email =
    tryGit(root, "--local", "user.email") || tryGit(root, "--global", "user.email");
  if (!email) return "unknown";
  return sanitizeSeg(email.replace(/@/g, "-at-"));
}

function sanitizeSeg(s: string): string {
  return (
    s.toLowerCase()
      .replace(/[^a-z0-9._-]+/g, "-")
      .replace(/-+/g, "-")
      .replace(/^-|-$/g, "")
      .slice(0, 80) || "unknown"
  );
}

// ---------------------------------------------------------------------------
// Active session + state resolution
// ---------------------------------------------------------------------------

interface ActiveSessionPointer {
  version: number;
  task_id: string;
  session_segment: string;
}

interface StateMinimal {
  task_id: string;
  step_index: number;
  flat_steps: Array<{ flat_id: string; title?: string }>;
}

function readActiveSessionPointer(root: string): ActiveSessionPointer | null {
  try {
    const raw = fs.readFileSync(path.join(root, ".vibe", "active.json"), "utf8");
    const obj = JSON.parse(raw) as ActiveSessionPointer;
    if (typeof obj.task_id !== "string" || !obj.task_id.trim()) return null;
    if (typeof obj.session_segment !== "string" || !obj.session_segment.trim()) return null;
    return obj;
  } catch {
    return null;
  }
}

function readSessionState(root: string, sessionSegment: string): StateMinimal | null {
  try {
    const statePath = path.join(root, ".vibe", "sessions", sessionSegment, "state.json");
    const raw = fs.readFileSync(statePath, "utf8");
    const obj = JSON.parse(raw) as StateMinimal;
    if (typeof obj.task_id !== "string") return null;
    if (!Array.isArray(obj.flat_steps)) return null;
    return obj;
  } catch {
    return null;
  }
}

// ---------------------------------------------------------------------------
// Error logging — writes to <platform-config>/hooks/logs/log-skill-read.log
// ---------------------------------------------------------------------------

function hookLogsDir(root: string, platform: string): string | null {
  if (platform === "claude") return path.join(root, ".claude", "hooks", "logs");
  if (platform === "cursor") return path.join(root, ".cursor", "hooks", "logs");
  return null;
}

function logError(root: string, platform: string, context: string, err: unknown): void {
  const logsDir = hookLogsDir(root, platform);
  if (!logsDir) return;
  try {
    fs.mkdirSync(logsDir, { recursive: true });
    const entry = JSON.stringify({
      ts: new Date().toISOString(),
      context,
      error: err instanceof Error ? err.message : String(err),
      stack: err instanceof Error ? err.stack : undefined,
    }, null, 2) + "\n\n";
    fs.appendFileSync(path.join(logsDir, "log-skill-read.log"), entry, "utf-8");
  } catch {
    // best-effort — never throw from error logger
  }
}

// ---------------------------------------------------------------------------
// Skill path detection
// ---------------------------------------------------------------------------

function extractReadPath(toolInput: unknown): string | null {
  if (typeof toolInput === "string") {
    try { toolInput = JSON.parse(toolInput); } catch { return null; }
  }
  if (!toolInput || typeof toolInput !== "object") return null;
  const obj = toolInput as Record<string, unknown>;
  for (const key of ["target_file", "file_path", "path", "absolute_path"]) {
    const v = obj[key];
    if (typeof v === "string" && v.trim()) return v.trim();
  }
  return null;
}

function isSkillFile(filePath: string): boolean {
  const norm = filePath.replace(/\\/g, "/");
  return norm.includes("SKILL.md") &&
    (norm.includes(".cursor/skills/") || norm.includes(".claude/skills/") || norm.includes(".vibe/skills/"));
}

// ---------------------------------------------------------------------------
// Per-step skill-reads.json — aggregated, one file per step
// ---------------------------------------------------------------------------

interface StepSkillReads {
  task_id: string;
  step_index: number;
  flat_id: string;
  title?: string;
  skills_loaded: SkillEntry[];
}

interface SkillEntry {
  ts: string;
  platform: string;
  source: string;
  skill_name?: string;
  skill_file?: string;
  args?: string | null;
  hook_event_name?: string;
  conversation_id?: string | null;
}

function stepSkillReadsPath(root: string, taskId: string, stepIndex: number, flatId: string): string {
  const slug = gitAuthorSlug(root);
  const taskSeg = sanitizeSeg(taskId);
  const flatSeg = sanitizeSeg(flatId);
  const stepDir = `${String(stepIndex).padStart(2, "0")}-${flatSeg}`;
  return path.join(vibeLogsDir(root), slug, taskSeg, "steps", stepDir, "skill-reads.json");
}

function updateStepSkillReads(root: string, platform: string, entry: Record<string, unknown>): void {
  const ptr = readActiveSessionPointer(root);
  if (!ptr) return;

  const state = readSessionState(root, ptr.session_segment);
  if (!state) return;

  const step = state.flat_steps[state.step_index];
  if (!step) return;

  const { flat_id, title } = step;
  const jsonPath = stepSkillReadsPath(root, state.task_id, state.step_index, flat_id);

  fs.mkdirSync(path.dirname(jsonPath), { recursive: true });

  let data: StepSkillReads;
  try {
    data = JSON.parse(fs.readFileSync(jsonPath, "utf8")) as StepSkillReads;
  } catch {
    data = {
      task_id: state.task_id,
      step_index: state.step_index,
      flat_id,
      title,
      skills_loaded: [],
    };
  }

  const skill: SkillEntry = {
    ts: new Date().toISOString(),
    platform: typeof entry.platform === "string" ? entry.platform : "unknown",
    source: typeof entry.source === "string" ? entry.source : "unknown",
  };
  if (typeof entry.skill_name === "string") skill.skill_name = entry.skill_name;
  if (typeof entry.skill_file === "string") skill.skill_file = entry.skill_file;
  if (entry.args !== undefined) skill.args = entry.args as string | null;
  if (typeof entry.hook_event_name === "string") skill.hook_event_name = entry.hook_event_name;
  if (entry.conversation_id !== undefined) skill.conversation_id = entry.conversation_id as string | null;

  data.skills_loaded.push(skill);

  const tmp = `${jsonPath}.tmp`;
  try {
    fs.writeFileSync(tmp, JSON.stringify(data, null, 2) + "\n", "utf-8");
    fs.renameSync(tmp, jsonPath);
  } catch (err) {
    logError(root, platform, "write_skill_reads_json", err);
  }
}

// ---------------------------------------------------------------------------
// Stdin reader
// ---------------------------------------------------------------------------

async function readStdin(): Promise<string> {
  const rl = readline.createInterface({ input: process.stdin });
  const lines: string[] = [];
  for await (const line of rl) lines.push(line);
  return lines.join("\n");
}

// ---------------------------------------------------------------------------
// Main
// ---------------------------------------------------------------------------

async function main(): Promise<void> {
  const root = projectRoot();
  const platform = detectPlatform();

  let payload: Payload;
  try {
    const raw = await readStdin();
    payload = JSON.parse(raw) as Payload;
  } catch (err) {
    logError(root, platform, "parse_stdin", err);
    process.stdout.write('{"permission":"allow"}\n');
    return;
  }

  const hookEvent = (payload.hook_event_name as string) ?? "";
  const toolName = (payload.tool_name as string) ?? "";

  const base = {
    hook_event_name: hookEvent,
    platform,
    conversation_id: payload.conversation_id ?? null,
  };

  let skillEntry: Record<string, unknown> | null = null;

  try {
    if (hookEvent === "UserPromptSubmit") {
      const prompt = ((payload.prompt as string) ?? "").trim();
      const m = prompt.match(/^\/([a-zA-Z][a-zA-Z0-9_-]*)(?:\s+(.*))?$/);
      if (m) {
        skillEntry = { ...base, source: "slash_command", skill_name: m[1], args: m[2] ?? null };
      }
    } else if (toolName === "Skill") {
      let toolInput = payload.tool_input ?? {};
      if (typeof toolInput === "string") {
        try { toolInput = JSON.parse(toolInput); } catch { toolInput = {}; }
      }
      const inp = toolInput as Record<string, unknown>;
      skillEntry = {
        ...base,
        source: "skill_tool",
        skill_name: String(inp.skill ?? inp.name ?? "unknown"),
        args: inp.args ?? null,
      };
    } else if (toolName === "Read") {
      const filePath = extractReadPath(payload.tool_input);
      if (filePath && isSkillFile(filePath)) {
        skillEntry = { ...base, source: "skill_file_read", skill_file: filePath };
      }
    }
  } catch (err) {
    logError(root, platform, "parse_payload", err);
  }

  if (skillEntry) {
    try {
      updateStepSkillReads(root, platform, skillEntry);
    } catch (err) {
      logError(root, platform, "update_step_skill_reads", err);
    }
  }

  process.stdout.write('{"permission":"allow"}\n');
}

main().catch((err) => {
  // Top-level catch: root/platform may not be resolvable, best-effort only
  try {
    const root = projectRoot();
    const platform = detectPlatform();
    logError(root, platform, "main_unhandled", err);
  } catch { /* ignore */ }
  process.stdout.write('{"permission":"allow"}\n');
});
