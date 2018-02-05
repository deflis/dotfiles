# users generic .zshrc file for zsh(1)
_DOTFILES=${HOME}/dotfiles

export XDG_CONFIG_HOME=${_DOTFILES}
export ZDOTDIR=${_DOTFILES}

export EDITOR=vim

export GOPATH=${HOME}/go

export path=(
    ${HOME}/go/bin(N-/)
    ${HOME}/bin(N-/)
    ${HOME}/.cabal/bin(N-/)
    $path
)

source ${_DOTFILES}/zshrc-path

typeset -U path cdpath fpath manpath ld_library_path include
