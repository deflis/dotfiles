# AGENTS.md

このファイルは、AIエージェント（Claude Code、GitHub Copilot等）がこのリポジトリで作業する際のガイドラインを提供します。

## リポジトリ概要

このリポジトリは[chezmoi](https://www.chezmoi.io/)で管理されているクロスプラットフォーム対応のdotfilesです。macOS、Linux、Windowsの各環境で統一的な設定管理を実現しています。

## 主な特徴

- **クロスプラットフォーム対応**: macOS、Linux、Windowsの3つのOSに対応
- **Goテンプレートエンジン**: プラットフォーム固有の設定をテンプレートで動的に生成
- **パッケージマネージャー統合**: Homebrew（macOS/Linux）、Scoop/winget（Windows）
- **複数シェル対応**: Zsh、PowerShell、Nushellの設定を管理
- **1Password統合**: SSHキー管理に1Password CLIを使用
- **自動更新**: 外部プラグインやテーマを自動ダウンロード・更新

## ファイル構造の基本ルール

### chezmoiの命名規則

- `dot_` プレフィックス → `.`で始まる隠しファイル（例: `dot_zshrc` → `.zshrc`）
- `.tmpl` 拡張子 → Goテンプレートとして処理される
- `private_` プレフィックス → プライベートファイル（権限600）
- `symlink_` プレフィックス → シンボリックリンクを作成
- `run_once_` プレフィックス → 初回のみ実行されるスクリプト
- `run_onchange_` プレフィックス → ファイル変更時に実行されるスクリプト

### ディレクトリ構造

```
.
├── .chezmoi.toml.tmpl          # chezmoi設定ファイル
├── .chezmoiexternal.toml       # 外部リソース定義
├── .chezmoiignore              # プラットフォーム別除外設定
├── Brewfile                    # Homebrewパッケージリスト
├── scoop.json                  # Scoopパッケージリスト
├── winget.json                 # wingetパッケージリスト
├── dot_zshenv.tmpl             # Zsh環境変数（エントリーポイント）
├── private_dot_config/
│   ├── zsh/                    # Zsh設定
│   │   ├── dot_zshrc           # メイン設定ファイル
│   │   ├── sync/               # 同期的に読み込まれる設定
│   │   ├── defer/              # 遅延読み込み設定
│   │   └── plugins/            # 外部プラグイン
│   ├── powershell/             # PowerShell設定
│   │   ├── profile.d/          # 即座に読み込まれる設定
│   │   └── defer.d/            # 遅延読み込み設定
│   ├── nushell/                # Nushell設定
│   ├── sheldon/                # Sheldonプラグインマネージャー設定
│   └── git/                    # Git設定
├── private_dot_ssh/            # SSH設定
│   ├── config                  # SSHクライアント設定
│   └── conf.d/                 # 分割された設定ファイル
└── run_onchange_before_*.tmpl  # パッケージマネージャー実行スクリプト
```

## 主要なファイルとその役割

### パッケージ管理

- **Brewfile**: macOS/Linux用のHomebrewパッケージリスト
  - 実行: `run_onchange_before_Brewfile.sh.cmd.tmpl`
  - OSチェック、Homebrewインストール、`brew bundle`実行

- **scoop.json**: Windows用のScoopパッケージリスト
  - 実行: `run_onchange_before_scoop.json.ps1.tmpl`
  - `scoop import`でインストール

- **winget.json**: Windows用のwingetパッケージリスト
  - 実行: `run_onchange_brefore_winget.json.ps1.tmpl`

### シェル設定

**Zsh（macOS/Linux）**:
- `dot_zshenv.tmpl`: 環境変数設定、`ZDOTDIR=$HOME/.config/zsh`を設定
- `private_dot_config/zsh/dot_zshrc`: メイン設定ファイル
  - `zsh-defer`を使用した遅延読み込み
  - `sync/`配下のファイルは即座に読み込み
  - `defer/`配下のファイルは`zsh-defer`で遅延読み込み
  - プラグイン: autosuggestions、syntax-highlighting等

**PowerShell（Windows）**:
- `Documents/PowerShell/Microsoft.PowerShell_profile.ps1.tmpl`
  - `profile.d/`配下の.ps1ファイルを即座に実行
  - `defer.d/`配下のファイルは非同期で読み込み

**Nushell（クロスプラットフォーム）**:
- `private_dot_config/nushell/config.nu`
- `private_dot_config/nushell/env.nu.tmpl`

### 開発ツール

**mise（旧rtx）**: バージョン管理ツール
- Node.js、Python、Ruby等のランタイム管理
- shims: `~/.local/share/mise/shims`
- 各シェルで初期化スクリプトを実行

**oh-my-posh**: プロンプトテーマエンジン
- テーマファイル: `private_dot_config/powershell/theme.yaml`
- 各シェルで初期化

**zoxide**: スマートcdコマンド
- 各シェルで初期化スクリプトを実行

### 1Password統合

- SSH agent: `~/.1password/agent.sock`または`~/Library/Group Containers/2BUA8C4S2C.com.1password/t/agent.sock`
- SSH設定: `private_dot_ssh/conf.d/onepassword.tmpl`
- `SSH_AUTH_SOCK`環境変数を自動設定

## テンプレート関数の使用

### プラットフォーム検出

```go
{{ if eq .chezmoi.os "darwin" }}
# macOS専用設定
{{ else if eq .chezmoi.os "linux" }}
# Linux専用設定
{{ else if eq .chezmoi.os "windows" }}
# Windows専用設定
{{ end }}
```

### よく使う関数

- `{{ lookPath "command" }}`: コマンドの存在チェック
- `{{ stat "path" }}`: ファイル/ディレクトリの存在チェック
- `{{ output "command" "args" }}`: コマンド実行と出力取得
- `{{ include "filename" }}`: ファイルの内容をインクルード
- `{{ .chezmoi.hostname }}`: ホスト名取得
- `{{ .chezmoi.username }}`: ユーザー名取得

## 開発ワークフロー

### テンプレートのテスト

```bash
# 適用前のドライラン
chezmoi apply --dry-run --verbose

# 特定ファイルのプレビュー
chezmoi cat ~/.zshrc

# テンプレートの実行テスト
chezmoi execute-template < file.tmpl
```

### パッケージの追加

**Homebrew**:
```bash
# Brewfileを直接編集
vim Brewfile
# chezmoiで適用（自動的にbrew bundleが実行される）
chezmoi apply
```

**Scoop**:
```bash
# 現在の状態をエクスポート
scoop export > scoop.json
# chezmoiに追加
chezmoi add scoop.json
```

### ファイルの編集

```bash
# chezmoiで管理されているファイルを編集
chezmoi edit ~/.zshrc

# 差分確認
chezmoi diff

# 適用
chezmoi apply

# 新しいファイルをchezmoiに追加
chezmoi add ~/.newconfig
```

### スクリプトの再実行

```bash
# スクリプトの状態をリセット（再実行させる）
chezmoi state delete-bucket --bucket=scriptState
chezmoi apply
```

## プラットフォーム固有の除外設定

`.chezmoiignore`でプラットフォーム別にファイルを除外:

- **Windows以外**: AppData、Documents/PowerShell等を除外
- **macOS以外**: Libraryディレクトリを除外
- **Windows**: Unix系ツール（.asdf、.zshrc、.tmux等）を除外

## 重要な注意事項

### 自動コミット

- `git.autoAdd`と`git.autoCommit`が有効な場合、変更は自動的にコミットされます
- `.chezmoi.toml.tmpl`で設定

### run_onchangeスクリプトのトリガー

- ファイル変更を検知するため、コメントでファイルをインクルードする「ハック」を使用
- 例: `{{ include "Brewfile"| comment "# " }}`

### 手動インストールが必要なもの

- フォント: HackGen、Hasklig
- Windows Terminalの設定（参考用のみ）

### Linuxbrew zshの有効化

```bash
which zsh | sudo tee -a /etc/shells
chsh -s `which zsh`
```

## トラブルシューティング

### テンプレートエラー

```bash
# 詳細なエラー表示
chezmoi apply --verbose

# 特定のファイルをデバッグ
chezmoi execute-template --init < .chezmoi.toml.tmpl
```

### パッケージマネージャーの問題

```bash
# Homebrewの状態確認
brew doctor

# Scoopの状態確認
scoop status

# 手動でのパッケージインストール
brew bundle --file=Brewfile
scoop import scoop.json
```

## 参考リンク

- [chezmoi公式ドキュメント](https://www.chezmoi.io/)
- [chezmoiテンプレート関数リファレンス](.github/copilot-instructions.md)
- [Go text/templateドキュメント](https://pkg.go.dev/text/template)
