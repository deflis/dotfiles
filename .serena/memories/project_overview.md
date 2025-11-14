# Project Overview
- Chezmoi-based dotfiles repo managing cross-platform (macOS, Linux, Windows) configs with Go templates.
- Tracks shell setups (Zsh, PowerShell, Nushell), package manager manifests (Homebrew, Scoop, winget), and SSH/git settings.
- Automates plugin/theme downloads, 1Password SSH agent integration, and runtime tooling (mise, oh-my-posh, zoxide).
- Auto-add/auto-commit is enabled in `.chezmoi.toml.tmpl`, so applying changes can trigger automatic git commits.
- Key directories: `private_dot_config/` (shell configs, git, sheldon, etc.), `private_dot_ssh/` (SSH config pieces), `Documents/` (PowerShell profiles), platform-specific resources like `AppData/` and `private_Library/`.