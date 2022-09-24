[ -d /usr/local/Library/Contributions ] && fpath=(/usr/local/Library/Contributions ${fpath})
[ -d /usr/local/share/zsh-completions ] && fpath=(/usr/local/share/zsh-completions $fpath)
if type brew &>/dev/null; then
[ -d $(brew --prefix)/share/zsh/site-functions ] && fpath=($(brew --prefix)/share/zsh/site-functions $fpath)
fi
source "$(chezmoi completion bash)"
