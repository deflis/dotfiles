# users generic .zshrc file for zsh(1)
_DOTFILES=${HOME}/dotfiles

export XDG_CONFIG_HOME=${_DOTFILES}
export ZDOTDIR=${_DOTFILES}

export EDITOR=vim
export LESS='--tabs=4 --no-init --LONG-PROMPT --ignore-case -R'

export GOPATH=${HOME}/go

export path=(
    ${HOME}/go/bin(N-/)
    ${HOME}/bin(N-/)
    ${HOME}/.cabal/bin(N-/)
    $pathh
)

typeset -U path cdpath fpath manpath ld_library_path include
