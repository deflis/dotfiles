export EDITOR=vim
export LESS='--tabs=4 --no-init --LONG-PROMPT --ignore-case -R'

[ -d ${HOME}/bin ] && export PATH=${PATH}:${HOME}/bin
[ -d ${HOME}/.cabal/bin ] && export PATH=${PATH}:${HOME}/.cabal/bin
export GOPATH=~/go
export path=(~/go/bin $path)
