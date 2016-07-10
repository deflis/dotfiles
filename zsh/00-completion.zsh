[ -d /usr/local/Library/Contributions ] && fpath=(/usr/local/Library/Contributions ${fpath})
[ -d /usr/local/share/zsh-completions ] && fpath=(/usr/local/share/zsh-completions $fpath)
fpath=(${HOME}/.zsh/functions/Completion ${fpath})
autoload -U compinit
compinit -u
zstyle ':completion:*' matcher-list '' 'm:{a-z}={A-Z}' '+m:{A-Z}={a-z}'
