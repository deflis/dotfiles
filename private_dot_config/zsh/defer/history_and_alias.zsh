## Command history configuration
#
HISTFILE=${HOME}/.zsh_history
HISTSIZE=50000
SAVEHIST=50000
setopt hist_ignore_space    # ignore 
setopt hist_ignore_dups     # ignore duplication command history list
setopt share_history        # share command history data

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
