## Alias configuration
setopt complete_aliases     # aliased ls needs if file/dir completions work

alias where="command -v"
alias j="jobs -l"

case "${OSTYPE}" in
freebsd*|darwin*)
    if (( $+commands[gls] )) ; then
        alias ls="gls --color"
    else
        alias ls="ls -G -w"
    fi
    ;;
linux*)
    alias ls="ls --color"
    ;;
esac

alias la="ls -a"
alias lf="ls -F"
alias ll="ls -al"

alias du="du -h"
alias df="df -h"

alias su="su -l"

export LESS='--tabs=4 --no-init --LONG-PROMPT --ignore-case -R'

cdr() {
  local dir=$(ghq list -p | fzf)
  if [[ -n "$dir" ]]; then
    cd "$dir"
  fi
}
alias tailf="tail -f"
