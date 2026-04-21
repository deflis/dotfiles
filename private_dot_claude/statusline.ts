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
  bg: PaletteName;
  fg: PaletteName;
  barEmpty: PaletteName;
};

// oh-my-posh 風のセグメント定義。type で種類を指定、foreground/background は
// theme.yaml パレット名で指定、segment 固有の設定は properties に入れる。
// separator はこのセグメントと次のセグメントの間に描く区切り文字 (powerline_symbol
// 相当)。最後のセグメントの場合は行末記号になる。省略時は SEPARATORS.solidRight。
type SegmentConfig =
  | {
    type: "cwd";
    foreground: PaletteName;
    background: PaletteName;
    separator?: string;
    properties?: {
      folderSeparator?: string;        // path 要素間の区切り (default SEPARATORS.thinRight)
      folderSeparatorColor?: PaletteName; // 同 色 (default foreground)
      maxParts?: number;               // 何要素を超えたら中略するか (default 4)
    };
  }
  | {
    type: "context";
    // foreground / background を指定すると、残量しきい値に関係なく固定される。
    // 省略時は properties.steps の該当 step.fg / step.bg が使われる。
    foreground?: PaletteName;
    background?: PaletteName;
    separator?: string;
    properties: {
      steps: readonly UsageStep[];
    };
  }
  | {
    type: "rate";
    // foreground / background を指定すると固定。省略時は step.fg / step.bg。
    // バーの残容量色 (barEmpty) は常に step から動的に決まる。
    foreground?: PaletteName;
    background?: PaletteName;
    separator?: string;
    properties: {
      scope: "five_hour" | "seven_day";
      icon: string;
      label: string;
      postfix?: string;
      showRemain?: "none" | "min" | "dhm";
      barFill: PaletteName;
      steps: readonly UsageStep[];
    };
  }
  | {
    type: "model";
    foreground: PaletteName;
    background: PaletteName;
    separator?: string;
  };

// ============================================================================
// 2. CONFIG  ←← ここを編集
// ============================================================================

// 2.1 theme.yaml からパレットを読む (見つからなければ fallback)
const THEME_YAML_CANDIDATES = [
  `${process.env.HOME ?? ""}/.config/powershell/theme.yaml`,
  `${process.env.HOME ?? ""}/.local/share/chezmoi/private_dot_config/powershell/theme.yaml`,
];

async function loadThemePalette(): Promise<ThemePalette> {
  for (const path of THEME_YAML_CANDIDATES) {
    try {
      const raw = await Bun.file(path).text();
      const block = /^palette:\s*\n((?:[ \t]+.*\n?)+)/m.exec(raw);
      if (!block) continue;
      const palette = {} as ThemePalette;
      for (const line of block[1]!.split("\n")) {
        const trimmed = line.trim();
        if (!trimmed || trimmed.startsWith("#")) continue;
        const mm = /^\s+([\w-]+):\s*"?(#[0-9a-fA-F]{3,8})"?/.exec(line);
        if (mm) palette[mm[1] as PaletteName] = mm[2]!;
      }
      if (Object.keys(palette).length > 0) return palette;
    } catch {
      // 次候補へ
    }
  }
  return FALLBACK_PALETTE;
}

// theme.yaml が無い/壊れているときの最低限の色 (Hue 360)
const FALLBACK_PALETTE: ThemePalette = {
  "black": "#111111",
  "blue": "#3261ab",
  "cyan": "#007fb1",
  "dark-blue": "#142744",
  "dark-cyan": "#003347",
  "dark-green": "#0e4506",
  "dark-lime": "#565a07",
  "dark-magenta": "#4c0c23",
  "dark-orange": "#5f4504",
  "dark-red": "#500e17",
  "dark-yellow": "#665c00",
  "dark-yelloworange": "#625002",
  "green": "#23ac0e",
  "light-blue": "#d5e0f1",
  "light-cyan": "#cae7f2",
  "light-green": "#d1f1cc",
  "light-lime": "#eef5d3",
  "light-magenta": "#f4d2de",
  "light-orange": "#f9dfd5",
  "light-red": "#f6d4d8",
  "light-yellow": "#fffbd5",
  "light-yelloworange": "#fcf1d3",
  "lime": "#a4c520",
  "magenta": "#bf1e56",
  "orange": "#da5019",
  "red": "#c7243a",
  "white": "#d6deeb",
  "yellow": "#ffe600",
  "yelloworange": "#edad0b",
};

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
//     theme.yaml の右側 (time / executiontime) と同じく
//     「dark-<色> bg + white fg」基調。安全域は blue 系、警告域は段階的に
//     yelloworange → orange → red と寄せる。緑は使わない (目に痛いため)。
//     barEmpty = バーの残容量部分に使う色 (dark-bg 上に浮かぶ同系統の基本色)。
const USAGE_STEPS: readonly UsageStep[] = [
  { minRemain: 50, bg: "dark-blue",         fg: "white", barEmpty: "blue" },
  { minRemain: 25, bg: "dark-yelloworange", fg: "light-yelloworange", barEmpty: "dark-yelloworange" },
  { minRemain: 10, bg: "dark-orange",       fg: "light-orange", barEmpty: "dark-orange" },
  { minRemain: 0,  bg: "dark-red",          fg: "light-red", barEmpty: "dark-red" },
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
  // 5h rate limit (残時間つき) — bg は固定、残量はバー色と数値で示す
  {
    type: "rate",
    foreground: "dark-magenta",
    background: "light-magenta",
    separator: SEPARATORS.roundRight,
    properties: {
      scope: "five_hour",
      icon: ICONS.timer,
      label: "",
      showRemain: "min",
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
    properties: { steps: USAGE_STEPS },
  },
  // 7d rate limit (残時間なし) — bg 固定
  {
    type: "rate",
    foreground: "white",
    background: "dark-blue",
    separator: SEPARATORS.roundRight,
    properties: {
      scope: "seven_day",
      icon: ICONS.calendar,
      label: "",
      showRemain: "min",
      barFill: "white",
      steps: USAGE_STEPS,
    },
  },
  // model (theme.yaml time segment 相当 = 末尾セグメント)
  // separator は最後なら行末記号として使われる (省略時は END_SYMBOL)
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

// theme.yaml のキー名 (kebab-case) で色を引く。無ければマゼンタ (目立つエラー色)
function p(key: PaletteName): string {
  return THEME[key] ?? "#ff00ff";
}

function hex(color: PaletteName | string): RGB {
  const c = color.startsWith("#") ? color : p(color as PaletteName);
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
      // 中間: このセグメントの separator を使う (省略時は solid right)
      const sep = s.separator ?? DEFAULT_SEPARATOR;
      out += bg(next.bg) + fg(s.bg) + sep;
    } else {
      // 末尾: separator が指定されていればそれ、無ければ END_SYMBOL
      const endSym = s.separator ?? END_SYMBOL;
      out += RESET + fg(s.bg) + endSym + RESET;
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
  overlay?: string,
): string {
  const clamped = Math.max(0, Math.min(100, usedPct));
  const fillCells = Math.max(
    0,
    Math.min(BAR_WIDTH, Math.round((clamped / 100) * BAR_WIDTH)),
  );
  const chars = [...(overlay ?? "")];
  const start = Math.floor((BAR_WIDTH - chars.length) / 2);

  let out = "";
  for (let i = 0; i < BAR_WIDTH; i++) {
    const isFilled = i < fillCells;
    const idx = i - start;
    if (idx >= 0 && idx < chars.length) {
      // overlay cell: bg=バーの色。fg は塗り上=黒、空き上=filledFg (通常は白) で
      // コントラスト確保。seg.background が light 色でも読めるよう、segBg は参照しない。
      const cellBg = isFilled ? filledFg : emptyFg;
      const cellFg = isFilled ? hex("black") : filledFg;
      out += bg(cellBg) + fg(cellFg) + chars[idx]!;
    } else {
      const cellFg = isFilled ? filledFg : emptyFg;
      out += bg(segBg) + fg(cellFg) + BAR_CHAR;
    }
  }
  // セル単位で変えた bg をセグメント背景に戻す
  return out + bg(segBg);
}

// mode:
//   "min" — 最小表記。最大単位だけ ("6d" / "27h" / "59m" / "<1m")
//   "dhm" — 複数単位連結 ("6d22h" / "1h30m" / "45m" / "<1m")
function formatRemain(
  resetsAt: number | undefined,
  mode: "min" | "dhm",
): string | null {
  if (typeof resetsAt !== "number") return null;
  const ms = resetsAt > 1e12 ? resetsAt : resetsAt * 1000;
  const diffMs = ms - Date.now();
  if (diffMs <= 0) return "<1m";

  const totalM = Math.floor(diffMs / 60_000);
  const d = Math.floor(totalM / (24 * 60));
  const h = Math.floor((totalM - d * 24 * 60) / 60);
  const m = totalM % 60;

  if (mode === "min") {
    if (d > 0) return `${d}d`;
    if (h > 0) return `${h}h`;
    return totalM > 0 ? `${totalM}m` : "<1m";
  }
  // dhm
  if (d > 0) return `${d}d${h.toString().padStart(2, "0")}h`;
  if (h > 0) return `${h}h${m.toString().padStart(2, "0")}m`;
  return totalM > 0 ? `${totalM}m` : "<1m";
}

function shortModel(m?: { id?: string; display_name?: string }): string {
  const raw = m?.id ?? m?.display_name ?? "";
  const re = /claude-(opus|sonnet|haiku)-(\d+)-(\d+)(?:\[(1m)\])?/i;
  const mm = re.exec(raw);
  if (mm) {
    const init = mm[1]![0]!.toUpperCase();
    const star = mm[4] ? "*" : "";
    return `${init}${mm[2]}.${mm[3]}${star}`;
  }
  // fallback: "Opus 4.7 (1M context)" -> "O4.7*"
  const dn = m?.display_name ?? "";
  const mm2 = /(Opus|Sonnet|Haiku)\s+(\d+)\.(\d+)(.*1M.*)?/i.exec(dn);
  if (mm2) {
    return `${mm2[1]![0]!.toUpperCase()}${mm2[2]}.${mm2[3]}${mm2[4] ? "*" : ""}`;
  }
  return m?.display_name ?? "Claude";
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
  const maxParts = seg.properties?.maxParts ?? 4;
  const folderSep = seg.properties?.folderSeparator ?? SEPARATORS.thinRight;

  let parts: string[];
  if (
    projectDir &&
    (current === projectDir || current.startsWith(projectDir + "/"))
  ) {
    const projectName =
      projectDir.split("/").filter(Boolean).pop() ?? projectDir;
    const rest = current.slice(projectDir.length).split("/").filter(Boolean);
    parts = [projectName, ...rest];
  } else if (home && (current === home || current.startsWith(home + "/"))) {
    const rest = current.slice(home.length).split("/").filter(Boolean);
    parts = ["~", ...rest];
  } else {
    parts = current.split("/").filter(Boolean);
    if (parts.length === 0) parts = ["/"];
  }

  if (parts.length > maxParts) {
    parts = [
      parts[0]!,
      "\u2026",
      parts[parts.length - 2]!,
      parts[parts.length - 1]!,
    ];
  }

  const iconLead = parts[0] === "~" ? ICONS.home : ICONS.folder;
  const dirFg = hex(seg.foreground);
  const sepFg = hex(seg.properties?.folderSeparatorColor ?? seg.foreground);
  // theme.yaml path segment の folder_separator_icon と同じ構造: " <color>sep</color> "
  const sep = `${fg(sepFg)} ${folderSep} ${fg(dirFg)}`;
  const text = `${iconLead} ${parts.join(sep)}`;

  return {
    text,
    fg: dirFg,
    bg: hex(seg.background),
    separator: seg.separator,
  };
}

function renderContext(
  seg: Extract<SegmentConfig, { type: "context" }>,
  input: HookInput,
): Seg | null {
  const remain = input.context_window?.remaining_percentage;
  if (typeof remain !== "number") return null;
  const step = pickStep(remain, seg.properties.steps);
  const text = `${ICONS.context} ${Math.round(remain)}%`;
  return {
    text,
    fg: hex(seg.foreground ?? step.fg),
    bg: hex(seg.background ?? step.bg),
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
  const textFg = hex(seg.foreground ?? step.fg);
  const segBg = hex(seg.background ?? step.bg);
  // バー: 塗り=barFill / 空き=step.barEmpty、中央に "NN%" を重ねる。
  const barStr =
    bar(
      used,
      hex(seg.properties.barFill),
      hex(step.barEmpty),
      segBg,
      `${Math.round(used)}%`,
    ) + fg(textFg);
  
  const prefix = [seg.properties.icon, seg.properties.label].filter(Boolean).join(" ");
  const postfix = [seg.properties.postfix].filter(Boolean).join(" ");
  const pieces = [
    prefix,
    barStr,
  ];
  if (seg.properties.showRemain && seg.properties.showRemain !== "none") {
    const remain = formatRemain(r.resets_at, seg.properties.showRemain);
    if (remain) pieces.push(remain);
  }
  pieces.push(postfix);
  return {
    text: pieces.join(" "),
    fg: textFg,
    bg: segBg,
    separator: seg.separator,
  };
}

function renderModel(
  seg: Extract<SegmentConfig, { type: "model" }>,
  input: HookInput,
): Seg {
  const text = `${ICONS.robot} ${shortModel(input.model)}`;
  return {
    text,
    fg: hex(seg.foreground),
    bg: hex(seg.background),
    separator: seg.separator,
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
