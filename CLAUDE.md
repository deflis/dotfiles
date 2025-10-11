# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

日本語で返答してください。

> **Note**: For detailed repository structure, file organization, and development workflows, see @AGENTS.md

## Repository Overview

This is a cross-platform dotfiles repository managed by [chezmoi](https://www.chezmoi.io/). It provides unified configuration management across macOS, Linux, and Windows systems using Go templates for platform-specific customization.

**Key characteristics**:
- Cross-platform support (macOS, Linux, Windows)
- Package manager integration (Homebrew, Scoop, winget)
- Multi-shell configuration (Zsh, PowerShell, Nushell)
- 1Password SSH integration
- Lazy-loading patterns for optimal shell startup performance

## Architecture Patterns

### Template-Driven Configuration

This repository uses advanced chezmoi templating patterns:

**Conditional compilation**: Platform-specific code is conditionally included using `{{ if eq .chezmoi.os "..." }}`

**Dynamic command execution**: Shell initialization uses `{{ output "command" }}` to capture tool outputs at template time (e.g., `brew shellenv`)

**File inclusion with change detection**: `run_onchange_*` scripts embed target files in comments to trigger re-execution:
```go
{{ include "Brewfile" | comment "# " }}
```

### Lazy Loading Architecture

**Zsh**: Uses `zsh-defer` plugin for asynchronous loading
- `sync/` directory: Critical configs loaded synchronously
- `defer/` directory: Non-critical configs deferred via `zsh-defer source $file`
- Plugins loaded asynchronously to minimize startup time

**PowerShell**: Custom async loading using PowerShell Runspaces
- `profile.d/`: Synchronous loading for essential configs
- `defer.d/`: Asynchronous background loading via `[PowerShell]::Create()` + `BeginInvoke()`

### External Resource Management

`.chezmoiexternal.toml` pattern:
- Auto-downloads external dependencies (plugins, themes)
- Refresh period: 168 hours (weekly)
- Types: `file` (single file), `archive` (extracted with `stripComponents`)
- Example: Zsh plugins, tmux TPM, Kitty themes

### Multi-Stage Installation

Script execution order:
1. `run_onchange_before_*`: Package managers (Homebrew, Scoop, winget)
2. File deployment: Templates processed and files written
3. `run_onchange_after_*`: Post-installation tasks (e.g., Sheldon plugin compilation)
4. `run_once_after_*`: One-time setup (e.g., Git configuration)

## Key Template Functions

Common patterns in this repository:

```go
{{ lookPath "command" }}           // Check if command exists in PATH
{{ stat "path" }}                  // Check if file/directory exists
{{ output "command" "args" }}      // Execute command and capture output
{{ .chezmoi.os }}                  // Platform detection
{{ .chezmoi.hostname }}            // Host-specific configuration
{{ onepasswordRead "op://..." }}   // 1Password secret integration
```

For comprehensive template function reference, see `.github/copilot-instructions.md`.

## Platform-Specific Behaviors

### macOS
- Homebrew installation location: `/opt/homebrew`
- `path_helper` workaround in `dot_zshenv.tmpl`
- 1Password SSH agent: `~/Library/Group Containers/2BUA8C4S2C.com.1password/t/agent.sock`

### Linux
- Homebrew (Linuxbrew) location: `/home/linuxbrew/.linuxbrew`
- WSL2 kernel detection via `.chezmoi.kernel.osrelease`
- 1Password SSH agent: `~/.1password/agent.sock`

### Windows
- PowerShell profile location: `Documents/PowerShell/`
- Scoop/winget for package management
- Unix tools (.zshrc, .tmux, etc.) excluded via `.chezmoiignore`

## Development Guidelines

### Modifying Templates

1. **Test template syntax**: `chezmoi execute-template < file.tmpl`
2. **Preview changes**: `chezmoi diff` or `chezmoi cat ~/.target`
3. **Dry-run application**: `chezmoi apply --dry-run --verbose`
4. **Apply changes**: `chezmoi apply`

### Adding New Packages

**Principle**: Edit declarative package lists, let `run_onchange_*` scripts handle installation

- Homebrew: Edit `Brewfile` → `chezmoi apply` triggers `brew bundle`
- Scoop: Edit `scoop.json` → `chezmoi apply` triggers `scoop import`
- winget: Edit `winget.json` → `chezmoi apply` triggers `winget import`

### Shell Configuration Changes

**Zsh**:
- Critical configs → `private_dot_config/zsh/sync/*.zsh`
- Deferred configs → `private_dot_config/zsh/defer/*.zsh`

**PowerShell**:
- Immediate → `private_dot_config/powershell/profile.d/*.ps1`
- Deferred → `private_dot_config/powershell/defer.d/*.ps1`

## Important Behaviors

- **Auto-commit enabled**: Changes to source directory are automatically committed (configurable in `.chezmoi.toml.tmpl`)
- **Template caching**: Command outputs are cached during template execution
- **Idempotency**: All scripts should be idempotent; use `run_once_*` for one-time operations
- **Manual installations**: Fonts (HackGen, Hasklig) require manual installation

## Troubleshooting

**Template errors**: Run `chezmoi apply --verbose` to see detailed error messages and template execution flow

**Script re-execution**: Force script re-run with `chezmoi state delete-bucket --bucket=scriptState`

**Platform detection issues**: Verify with `chezmoi data` to inspect all template variables

## Reference Documentation

- Repository structure and workflows: [AGENTS.md](AGENTS.md)
- chezmoi template functions: `.github/copilot-instructions.md`
- Official chezmoi docs: https://www.chezmoi.io/
