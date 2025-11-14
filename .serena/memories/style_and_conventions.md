# Style and Conventions
- Formatting governed by `.editorconfig`: LF endings, final newline, 2-space indentation; Windows scripts (`*.cmd`, `*.ps1`, related templates) expect CRLF.
- Follow chezmoi naming rules (`dot_`, `private_`, `symlink_`, `run_once_`, `run_onchange_`, `.tmpl`) to control target paths, permissions, and template execution.
- Templates use Go text/template syntax; prefer conditional blocks (`{{ if eq .chezmoi.os "darwin" }}` etc.) for platform-specific logic and helper functions like `lookPath`, `stat`, `output`, `include`.
- Shell scripts favor idiomatic Zsh/Bash, Windows scripts use PowerShell Cmdlets; keep comments meaningful and concise.