## Alias configuration
#
# expand aliases before completing
#

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

alias emacs=vim
alias tmuxx='~/dotfiles/tmuxx.sh'
alias tailf="tail -f"

if [ -n `whence mvim` ] ; then
    alias gvim=mvim
elif [ -n `whence gvim` ] ; then
    alias mvim=gvim
else
    alias gvim=vim
    alias mvim=vim
fi;

