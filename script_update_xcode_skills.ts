#!/usr/bin/env bun
// Xcodeが提供するエージェント向けスキルを対話的に選んで ~/.agents/skills へ入れるメンテスクリプト。
// - `xcrun agent skills export` の結果から、入れるスキルをfzfで選ぶ（今入っているものが選択済み）
// - 続けて ~/.claude/skills にリンクするスキルを選ぶ（今リンクしているものが選択済み）
// - 変更内容を確認してから実行する。選ばなかったスキルやXcodeが提供しなくなったスキル
//   （chezmoi管理外のもの）はゴミ箱へ移動する
//
// USAGE: ./script_update_xcode_skills.ts

import { $ } from "bun";
import { cp, lstat, mkdir, mkdtemp, readdir, rm, stat, symlink } from "node:fs/promises";
import { homedir, tmpdir } from "node:os";
import { join } from "node:path";

const SKILLS_DIR = join(homedir(), ".agents", "skills");
const CLAUDE_SKILLS_DIR = join(homedir(), ".claude", "skills");

type State = "新規" | "更新あり" | "最新" | "提供終了";
type Candidate = { name: string; state: string };

function fail(message: string): never {
  console.error(message);
  process.exit(1);
}

async function exists(path: string): Promise<boolean> {
  return stat(path).then(
    () => true,
    () => false,
  );
}

async function isSymlink(path: string): Promise<boolean> {
  return lstat(path).then(
    (s) => s.isSymbolicLink(),
    () => false,
  );
}

async function listDirs(path: string): Promise<string[]> {
  const entries = await readdir(path, { withFileTypes: true }).catch(() => []);
  return entries.filter((e) => e.isDirectory()).map((e) => e.name).sort();
}

async function isSameDir(a: string, b: string): Promise<boolean> {
  const { exitCode } = await $`diff -rq -x .DS_Store ${a} ${b}`.quiet().nothrow();
  return exitCode === 0;
}

/**
 * fzfで複数選択させる。選んだ名前を返し、キャンセルされたら null を返す。
 * @param preselected 最初から選択済みにする名前
 */
async function selectSkills(
  prompt: string,
  candidates: Candidate[],
  preselected: string[],
  previewDirs: string[],
): Promise<string[] | null> {
  const selectActions = candidates
    .map((c, i) => (preselected.includes(c.name) ? `pos(${i + 1})+select+` : ""))
    .join("");
  const preview = previewDirs.map((dir) => `cat '${dir}/{1}/SKILL.md' 2>/dev/null`).join(" || ");

  const proc = Bun.spawn(
    [
      "fzf",
      "--multi",
      "--delimiter=\t",
      "--accept-nth=1",
      `--prompt=${prompt}> `,
      "--header=TAB: 選択切り替え / Enter: 決定 / Esc: 中止",
      `--bind=load:${selectActions}first`,
      // 何も選ばずにEnterするとカーソル行が選ばれてしまうので、選択0件のときは空で終わらせる
      `--bind=enter:transform:[ "$FZF_SELECT_COUNT" -eq 0 ] && echo "become(true)" || echo accept`,
      `--preview=${preview}`,
      "--preview-window=right,60%,wrap",
    ],
    { stdin: "pipe", stdout: "pipe", stderr: "inherit" },
  );
  proc.stdin.write(candidates.map((c) => `${c.name}\t${c.state}\n`).join(""));
  proc.stdin.end();

  const output = await new Response(proc.stdout).text();
  if ((await proc.exited) !== 0) return null;
  return output.split("\n").filter(Boolean);
}

if (!process.stdin.isTTY || !process.stdout.isTTY) fail("対話的に実行してください。");
for (const cmd of ["fzf", "chezmoi", "trash"]) {
  if (!Bun.which(cmd)) fail(`${cmd} が見つかりません。`);
}
if ((await $`xcrun --find mcpbridge`.quiet().nothrow()).exitCode !== 0) {
  fail("xcrun agent が使えません。Xcodeがインストールされているか、xcode-select の設定を確認してください。");
}

const tmpDir = await mkdtemp(join(tmpdir(), "xcode-skills-"));
const exportDir = join(tmpDir, "export");
process.on("exit", () => {
  Bun.spawnSync(["rm", "-rf", tmpDir]);
});

console.log("==> Xcodeのスキルを書き出しています...");
await $`xcrun agent skills export --output-dir ${exportDir}`.quiet();
const exported = await listDirs(exportDir);
if (exported.length === 0) fail("書き出されたスキルがありません。中断します。");

// chezmoiで管理している自作スキルは対象から外す
const managed = (await $`chezmoi managed --include=dirs --path-style=absolute`.text())
  .split("\n")
  .filter((path) => path.startsWith(`${SKILLS_DIR}/`))
  .map((path) => path.slice(SKILLS_DIR.length + 1))
  .filter((name) => !name.includes("/"));

// 今 ~/.agents/skills に入っているXcodeのスキル（提供終了したものも含む）
const installed = (await listDirs(SKILLS_DIR)).filter((name) => !managed.includes(name));

// 今 ~/.claude/skills にリンクしているXcodeのスキル
const linked: string[] = [];
for (const name of new Set([...installed, ...exported])) {
  if (await isSymlink(join(CLAUDE_SKILLS_DIR, name))) linked.push(name);
}

const agentCandidates: { name: string; state: State }[] = [];
for (const name of exported) {
  let state: State;
  if (!installed.includes(name)) state = "新規";
  else if (await isSameDir(join(exportDir, name), join(SKILLS_DIR, name))) state = "最新";
  else state = "更新あり";
  agentCandidates.push({ name, state });
}
for (const name of installed) {
  if (!exported.includes(name)) agentCandidates.push({ name, state: "提供終了" });
}

function abort(): never {
  fail("中止しました。");
}

// 選び直せるように、前回の選択を次の初期選択にする
// 提供終了したスキルは消す前提なので、最初は選択しない
let selectedAgents = installed.filter((name) => exported.includes(name));
let selectedClaude = linked;
let plan!: { copy: string[]; trash: string[]; link: string[]; unlink: string[] };

while (true) {
  // 1. ~/.agents/skills に入れるスキルを選ぶ
  selectedAgents =
    (await selectSkills("~/.agents/skills に入れるスキル", agentCandidates, selectedAgents, [
      exportDir,
      SKILLS_DIR,
    ])) ?? abort();

  // 2. ~/.claude/skills にリンクするスキルを選ぶ
  if (selectedAgents.length > 0) {
    const claudeCandidates = selectedAgents.map((name) => ({
      name,
      state: linked.includes(name) ? "リンク済み" : "",
    }));
    selectedClaude =
      (await selectSkills("~/.claude/skills にリンクするスキル", claudeCandidates, selectedClaude, [
        exportDir,
        SKILLS_DIR,
      ])) ?? abort();
  } else {
    selectedClaude = [];
  }

  // 3. 変更内容をまとめる
  plan = {
    copy: agentCandidates
      .filter((c) => selectedAgents.includes(c.name) && (c.state === "新規" || c.state === "更新あり"))
      .map((c) => c.name),
    trash: installed.filter((name) => !selectedAgents.includes(name)),
    link: selectedClaude.filter((name) => !linked.includes(name)),
    unlink: linked.filter((name) => !selectedClaude.includes(name)),
  };

  if (Object.values(plan).every((names) => names.length === 0)) {
    console.log("変更はありません。");
    process.exit(0);
  }

  // 4. 変更内容を確認する
  console.log("\n==> 変更内容");
  for (const name of plan.copy) console.log(`  + ~/.agents/skills/${name} を入れる/更新する`);
  for (const name of plan.trash) console.log(`  - ~/.agents/skills/${name} をゴミ箱へ移動する`);
  for (const name of plan.link) console.log(`  + ~/.claude/skills/${name} にリンクする`);
  for (const name of plan.unlink) console.log(`  - ~/.claude/skills/${name} のリンクを外す`);
  console.log();

  const answer = prompt("実行しますか? [y: 実行 / r: 選び直す / N: 中止]")?.trim().toLowerCase();
  if (answer === "y") break;
  if (answer !== "r") abort();
}

// リンクを先に外してから実体を消す
for (const name of plan.unlink) {
  await rm(join(CLAUDE_SKILLS_DIR, name));
}
for (const name of plan.trash) {
  await $`trash ${join(SKILLS_DIR, name)}`;
}
if (plan.copy.length > 0) {
  await mkdir(SKILLS_DIR, { recursive: true });
  for (const name of plan.copy) {
    const target = join(SKILLS_DIR, name);
    await rm(target, { recursive: true, force: true });
    await cp(join(exportDir, name), target, { recursive: true });
  }
}
if (plan.link.length > 0) {
  await mkdir(CLAUDE_SKILLS_DIR, { recursive: true });
  for (const name of plan.link) {
    await symlink(`../../.agents/skills/${name}`, join(CLAUDE_SKILLS_DIR, name));
  }
}

// ~/.agents/skills のスキルを消したことで切れたリンクを掃除する
for (const entry of await readdir(CLAUDE_SKILLS_DIR).catch(() => [])) {
  const link = join(CLAUDE_SKILLS_DIR, entry);
  if ((await isSymlink(link)) && !(await exists(link))) {
    console.log(`==> リンク切れを削除: ${link}`);
    await rm(link);
  }
}

console.log("完了しました。");
