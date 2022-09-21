[ -d /usr/local/Library/Contributions ] && fpath=(/usr/local/Library/Contributions ${fpath})
[ -d /usr/local/share/zsh-completions ] && fpath=(/usr/local/share/zsh-completions $fpath)

eval "$(chezmoi completion zsh)"
