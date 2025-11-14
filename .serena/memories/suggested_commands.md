# Suggested Commands
- `chezmoi apply --dry-run --verbose` — preview changes before applying dotfiles.
- `chezmoi diff` / `chezmoi apply` — review and apply updates; note autoAdd/autoCommit will record changes.
- `chezmoi cat ~/.zshrc` — inspect rendered templates; swap target path as needed.
- `chezmoi execute-template < file.tmpl` — test template output directly.
- `chezmoi add <path>` — bring new local config under chezmoi control.
- `chezmoi state delete-bucket --bucket=scriptState` — reset run_once/run_onchange script state for re-execution.
- `brew bundle --file=Brewfile` / `scoop import scoop.json` / `winget import winget.json` — manually sync package manifests if automation fails.
- `brew doctor` / `scoop status` — troubleshoot package manager issues.