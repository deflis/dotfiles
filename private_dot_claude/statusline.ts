#!/usr/bin/env bun
/**
 * Claude Code statusline
 *
 * 編集ポイントは下の CONFIG セクションだけ。ENGINE セクションは触らなくていい。
 * SEGMENTS 配列が oh-my-posh の blocks[].segments に相当する。
 * 色は private_dot_config/powershell/theme.yaml (Hue 360 palette) から
 * 実行時に読み込む。theme.yaml を直接編集すれば statusline にも反映される。
 *
 * Docs: https://code.claude.com/docs/en/statusline
 */

// ============================================================================
// 1. TYPES
// ============================================================================

// Claude Code statusline hook input
// https://code.claude.com/docs/en/statusline#json-input-structure
type HookInput = {
  hook_event_name?: string;
  session_id?: string;
  transcript_path?: string;
  cwd?: string;
  model?: {
    id?: string;
    display_name?: string;
  };
  workspace?: {
    current_dir?: string;
    project_dir?: string;
  };
  version?: string;
  output_style?: {
    name?: string;
  };
  cost?: {
    total_cost_usd?: number;
    total_duration_ms?: number;
    total_api_duration_ms?: number;
    total_lines_added?: number;
    total_lines_removed?: number;
  };
  context_window?: {
    total_input_tokens?: number;
    total_output_tokens?: number;
    context_window_size?: number;
    used_percentage?: number;
    remaining_percentage?: number;
    current_usage?: {
      input_tokens?: number;
      output_tokens?: number;
      cache_creation_input_tokens?: number;
      cache_read_input_tokens?: number;
    };
  };
  rate_limits?: {
    five_hour?: {
      used_percentage?: number;
      resets_at?: number;
    };
    seven_day?: {
      used_percentage?: number;
      resets_at?: number;
    };
  };
};

type PaletteName =
  | "black"
  | "blue"
  | "cyan"
  | "dark-blue"
  | "dark-cyan"
  | "dark-green"
  | "dark-lime"
  | "dark-magenta"
  | "dark-orange"
  | "dark-red"
  | "dark-yellow"
  | "dark-yelloworange"
  | "green"
  | "light-blue"
  | "light-cyan"
  | "light-green"
  | "light-lime"
  | "light-magenta"
  | "light-orange"
  | "light-red"
  | "light-yellow"
  | "light-yelloworange"
  | "lime"
  | "magenta"
  | "orange"
  | "red"
  | "white"
  | "yellow"
  | "yelloworange"
  | `#${string}`; // theme.yaml にない色を直接指定する場合 (例: "##c15f3c")
type ThemePalette = Record<PaletteName, string>;

type UsageStep = {
  minRemain: number;
  barEmpty: PaletteName;
};

// oh-my-posh 風のセグメント定義。type で種類を指定、foreground/background は
// theme.yaml パレット名で指定、segment 固有の設定は properties に入れる。
// separator はこのセグメントと次のセグメントの間に描く区切り文字 (powerline_symbol
// 相当)。最後のセグメントの場合は行末記号になる。
// model 型は末尾固定で常に END_SYMBOL が使われる。
type SegmentConfig =
  | {
    type: "cwd";
    foreground: PaletteName;
    background: PaletteName;
    separator: string;
    properties: {
      folderSeparator: string;          // path 要素間の区切り
      folderSeparatorColor: PaletteName; // 同 色
      maxParts: number;                  // 何要素を超えたら中略するか
    };
  }
  | {
    type: "context";
    foreground: PaletteName;
    background: PaletteName;
    separator: string;
  }
  | {
    type: "rate";
    foreground: PaletteName;
    background: PaletteName;
    separator: string;
    properties: {
      scope: "five_hour" | "seven_day";
      icon: string;
      label: string;
      barFill: PaletteName;
      // バーの残容量色 (step.barEmpty) は使用率しきい値で動的に決まる。
      steps: readonly UsageStep[];
    };
  }
  | {
    type: "model";
    foreground: PaletteName;
    background: PaletteName;
  };

// ============================================================================
// 2. CONFIG  ←← ここを編集
// ============================================================================

// 2.1 theme.yaml からパレットを読む
const THEME_YAML_PATH = `${process.env.HOME!}/.config/powershell/theme.yaml`;

async function loadThemePalette(): Promise<ThemePalette> {
  const raw = await Bun.file(THEME_YAML_PATH).text();
  const block = /^palette:\s*\n((?:[ \t]+.*\n?)+)/m.exec(raw);
  if (!block) throw new Error(`palette block not found in ${THEME_YAML_PATH}`);
  const palette = {} as ThemePalette;
  for (const line of block[1]!.split("\n")) {
    const trimmed = line.trim();
    if (!trimmed || trimmed.startsWith("#")) continue;
    const mm = /^\s+([\w-]+):\s*"?(#[0-9a-fA-F]{3,8})"?/.exec(line);
    if (mm) palette[mm[1] as PaletteName] = mm[2]!;
  }
  return palette;
}

const THEME = await loadThemePalette();

// 2.2 Nerd Font アイコン (HackGen Console NFJ 前提)
const ICONS = {
  home: "\u{f015}",     // nf-fa-home
  folder: "\u{f07b}",   // nf-fa-folder
  context: "\u{f085b}", // nf-md-memory 󰡛
  timer: "\u{f051b}",   // nf-md-timer 󰔛
  calendar: "\u{f073}", // nf-fa-calendar
  robot: "\u{f06a9}",   // nf-md-robot 󰚩
} as const;

// 2.3 残り% のしきい値と配色 (上から順にマッチ)
//     barEmpty = バーの残容量部分に使う色。
const USAGE_STEPS: readonly UsageStep[] = [
  { minRemain: 50, barEmpty: "blue" },
  { minRemain: 25, barEmpty: "dark-yelloworange" },
  { minRemain: 10, barEmpty: "dark-orange" },
  { minRemain: 0,  barEmpty: "dark-red" },
];

// 2.4 Powerline 区切り文字 (Nerd Font)
//     SEGMENTS の separator や properties.folderSeparator で参照する。
//     自前のグリフ (" " や "│" 等) を指定することもできる。
const SEPARATORS = {
  solidRight: "\uE0B0", //  powerline right (塗り)
  thinRight:  "\uE0B1", //  powerline right (細)
  solidLeft:  "\uE0B2", //  powerline left (塗り)
  thinLeft:   "\uE0B3", //  powerline left (細)
  roundRight: "\uE0B4", //  半円 (右側)
  thinRound:  "\uE0B5", //  半円 (右側, 細)
  roundLeft:  "\uE0B6", //  半円 (左側)
  thinRoundL: "\uE0B7", //  半円 (左側, 細)
  flameR:     "\uE0C0", //  炎 (右)
  pixelR:     "\uE0C4", //  ドット (右)
  honeyR:     "\uE0CC", //  六角 (右)
} as const;

// 先頭キャップ / 中間セグメント右端 / 行末記号
const LEADING_SYMBOL = SEPARATORS.roundLeft;     // 先頭セグメントの左端 ("" で無効化)
const DEFAULT_SEPARATOR = SEPARATORS.solidRight; // 中間セグメントの右端
const END_SYMBOL = SEPARATORS.roundRight;        // 最終セグメントの右端 (行末)

// 2.5 その他の調整ノブ
const BAR_WIDTH = 10;
// 塗り/空きを同じ文字で描き、色で差をつける (dots 混じりだと黒く見えるため)
const BAR_CHAR = "\u2588"; // █

// 2.6 セグメント一覧 (順序 + 配色 + 区切り)
//     oh-my-posh の blocks[].segments と同じ感覚で書く。
//     行を入れ替えれば順序変更、行ごとコメントアウトで ON/OFF、
//     foreground/background を差し替えれば色替え、
//     separator を差し替えれば区切り文字変更。
const SEGMENTS: SegmentConfig[] = [
  // cwd (theme.yaml path segment 相当)
  {
    type: "cwd",
    foreground: "dark-blue",
    background: "light-blue",
    separator: SEPARATORS.roundRight,
    properties: {
      folderSeparator: SEPARATORS.thinRight, // path 要素間は thin
      folderSeparatorColor: "blue",
      maxParts: 4,
    },
  },
  // 5h rate limit — bg は固定、残量はバー色と数値で示す
  {
    type: "rate",
    foreground: "dark-magenta",
    background: "light-magenta",
    separator: SEPARATORS.roundRight,
    properties: {
      scope: "five_hour",
      icon: ICONS.timer,
      label: "",
      barFill: "white",
      steps: USAGE_STEPS,
    },
  },
  // context 残量 — bg 固定
  {
    type: "context",
    foreground: "white",
    background: "#c15f3c", // Claude Orange
    separator: SEPARATORS.roundRight,
  },
  // 7d rate limit — bg 固定
  {
    type: "rate",
    foreground: "white",
    background: "dark-blue",
    separator: SEPARATORS.roundRight,
    properties: {
      scope: "seven_day",
      icon: ICONS.calendar,
      label: "",
      barFill: "white",
      steps: USAGE_STEPS,
    },
  },
  // model (theme.yaml time segment 相当 = 末尾セグメント、行末は END_SYMBOL)
  {
    type: "model",
    foreground: "white",
    background: "blue",
  },
];

// ============================================================================
// 3. ENGINE  (通常触らなくていい)
// ============================================================================

// 3.1 ANSI helpers
type RGB = readonly [number, number, number];
type Seg = {
  text: string;
  fg: RGB;
  bg: RGB;
  separator?: string; // SegmentConfig.separator の解決後の値
};

const RESET = "\x1b[0m";
const fg = ([r, g, b]: RGB) => `\x1b[38;2;${r};${g};${b}m`;
const bg = ([r, g, b]: RGB) => `\x1b[48;2;${r};${g};${b}m`;

// theme.yaml のキー名 (kebab-case) で色を引く
function hex(color: PaletteName): RGB {
  const c = color.startsWith("#") ? color : THEME[color]!;
  const n = parseInt(c.slice(1), 16);
  return [(n >> 16) & 255, (n >> 8) & 255, n & 255] as const;
}

function render(segs: Seg[]): string {
  let out = "";
  for (let i = 0; i < segs.length; i++) {
    const s = segs[i]!;
    if (i === 0 && LEADING_SYMBOL) {
      // 先頭キャップ: セグメント bg の色で半円を描く (bg は端末デフォルト)
      out += fg(s.bg) + LEADING_SYMBOL;
    }
    out += bg(s.bg) + fg(s.fg) + ` ${s.text} `;
    const next = segs[i + 1];
    if (next) {
      // 中間: このセグメントの separator (model は未設定なので DEFAULT_SEPARATOR)
      out += bg(next.bg) + fg(s.bg) + (s.separator ?? DEFAULT_SEPARATOR);
    } else {
      // 末尾: separator があれば使い、無ければ END_SYMBOL (model 用)
      out += RESET + fg(s.bg) + (s.separator ?? END_SYMBOL) + RESET;
    }
  }
  return out;
}

function pickStep(
  remainPct: number,
  steps: readonly UsageStep[],
): UsageStep {
  for (const step of steps) {
    if (remainPct >= step.minRemain) return step;
  }
  return steps[steps.length - 1]!;
}

// overlay を渡すと、バー中央にその文字列を重ねる (例: "48%")。
// overlay セルは セル bg=バーの色 / fg=セグメント bg でコントラストを取る。
function bar(
  usedPct: number,
  filledFg: RGB,
  emptyFg: RGB,
  segBg: RGB,
  overlay: string,
): string {
  const clamped = Math.max(0, Math.min(100, usedPct));
  const fillCells = Math.max(
    0,
    Math.min(BAR_WIDTH, Math.round((clamped / 100) * BAR_WIDTH)),
  );
  const chars = [...(overlay)];
  const start = Math.floor((BAR_WIDTH - chars.length) / 2);

  let out = "";
  for (let i = 0; i < BAR_WIDTH; i++) {
    const isFilled = i < fillCells;
    const idx = i - start;
    if (idx >= 0 && idx < chars.length) {
      // overlay cell: bg=バーの色。fg は塗り上=黒、空き上=filledFg (通常は白) で
      // コントラスト確保。seg.background が light 色でも読めるよう、segBg は参照しない。
      out += bg(isFilled ? filledFg : emptyFg)
           + fg(isFilled ? hex("black") : filledFg)
           + chars[idx]!;
    } else {
      out += bg(segBg) + fg(isFilled ? filledFg : emptyFg) + BAR_CHAR;
    }
  }
  // セル単位で変えた bg をセグメント背景に戻す
  return out + bg(segBg);
}

// 最大単位だけの最小表記 ("6d" / "27h" / "59m" / "<1m")
// resets_at は Unix epoch seconds (Claude Code の statusline 仕様)
function formatRemain(resetsAt: number | undefined): string | null {
  if (typeof resetsAt !== "number") return null;
  const diffSec = resetsAt - Date.now() / 1000;

  const totalM = Math.floor(diffSec / 60);
  const d = Math.floor(totalM / (24 * 60));
  const h = Math.floor((totalM - d * 24 * 60) / 60);

  if (d > 0) return `${d}d`;
  if (h > 0) return `${h}h`;
  return totalM > 0 ? `${totalM}m` : "<1m";
}

function shortModel(m?: { id?: string; display_name?: string }): string {
  const raw = `${m?.id ?? ""} ${m?.display_name ?? ""}`;
  const mm = /(opus|sonnet|haiku)[\s-](\d+)[.\-](\d+)/i.exec(raw);
  if (!mm) return m?.display_name ?? "Claude";
  return `${mm[1]![0]!.toUpperCase()}${mm[2]}.${mm[3]}${/\[1m\]|1M/i.test(raw) ? "*" : ""}`;
}

// 3.2 Segment renderers (type ごとに 1 つ)

function renderCwd(
  seg: Extract<SegmentConfig, { type: "cwd" }>,
  input: HookInput,
): Seg | null {
  const current = input.cwd ?? input.workspace?.current_dir;
  if (!current) return null;
  const projectDir = input.workspace?.project_dir;
  const home = process.env.HOME ?? "";
  const { maxParts, folderSeparator, folderSeparatorColor } = seg.properties;

  const stripPrefix = (s: string, prefix: string): string[] | null =>
    s === prefix ? []
    : s.startsWith(prefix + "/") ? s.slice(prefix.length).split("/").filter(Boolean)
    : null;

  const projectRest = projectDir ? stripPrefix(current, projectDir) : null;
  const homeRest = home ? stripPrefix(current, home) : null;
  let parts: string[];
  if (projectRest) {
    const projectName = projectDir!.split("/").filter(Boolean).pop() ?? projectDir!;
    parts = [projectName, ...projectRest];
  } else if (homeRest) {
    parts = ["~", ...homeRest];
  } else {
    const abs = current.split("/").filter(Boolean);
    parts = abs.length ? abs : ["/"];
  }

  if (parts.length > maxParts) {
    parts = [
      parts[0]!,
      "\u2026",
      parts[parts.length - 2]!,
      parts[parts.length - 1]!,
    ];
  }

  const dirFg = hex(seg.foreground);
  // theme.yaml path segment の folder_separator_icon と同じ構造: " <color>sep</color> "
  const sep = `${fg(hex(folderSeparatorColor))} ${folderSeparator} ${fg(dirFg)}`;
  return {
    text: `${parts[0] === "~" ? ICONS.home : ICONS.folder} ${parts.join(sep)}`,
    fg: dirFg,
    bg: hex(seg.background),
    separator: seg.separator,
  };
}

function renderContext(
  seg: Extract<SegmentConfig, { type: "context" }>,
  input: HookInput,
): Seg | null {
  const used = input.context_window?.used_percentage;
  if (typeof used !== "number") return null;
  return {
    text: `${ICONS.context} ${Math.round(used)}%`,
    fg: hex(seg.foreground),
    bg: hex(seg.background),
    separator: seg.separator,
  };
}

function renderRate(
  seg: Extract<SegmentConfig, { type: "rate" }>,
  input: HookInput,
): Seg | null {
  const r = input.rate_limits?.[seg.properties.scope];
  if (!r) return null;
  const used = typeof r.used_percentage === "number" ? r.used_percentage : 0;
  const step = pickStep(100 - used, seg.properties.steps);
  const textFg = hex(seg.foreground);
  const segBg = hex(seg.background);
  // バー: 塗り=barFill / 空き=step.barEmpty、中央に "NN%" を重ねる。
  const barStr =
    bar(
      used,
      hex(seg.properties.barFill),
      hex(step.barEmpty),
      segBg,
      `${Math.round(used)}%`,
    ) + fg(textFg);

  const text = [
    seg.properties.icon,
    seg.properties.label,
    barStr,
    formatRemain(r.resets_at),
  ].filter(Boolean).join(" ");
  return {
    text,
    fg: textFg,
    bg: segBg,
    separator: seg.separator,
  };
}

function renderModel(
  seg: Extract<SegmentConfig, { type: "model" }>,
  input: HookInput,
): Seg {
  return {
    text: `${ICONS.robot} ${shortModel(input.model)}`,
    fg: hex(seg.foreground),
    bg: hex(seg.background),
  };
}

function renderSegment(cfg: SegmentConfig, input: HookInput): Seg | null {
  switch (cfg.type) {
    case "cwd":     return renderCwd(cfg, input);
    case "context": return renderContext(cfg, input);
    case "rate":    return renderRate(cfg, input);
    case "model":   return renderModel(cfg, input);
  }
}

// 3.3 main
const raw = await Bun.stdin.text();
let input: HookInput = {};
try {
  if (raw.trim().length > 0) input = JSON.parse(raw) as HookInput;
} catch {
  // 壊れた JSON が来ても statusline を死なせない
}
const segs = SEGMENTS
  .map((cfg) => renderSegment(cfg, input))
  .filter((s): s is Seg => s !== null);
if (segs.length > 0) process.stdout.write(render(segs));
