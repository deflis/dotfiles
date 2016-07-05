# users generic .zshrc file for zsh(1)
_DOTFILES=${HOME}/dotfiles

export XDG_CONFIG_HOME=${_DOTFILES}
## Environment variable configuration
#
# LANG
#
export LANG=en_US.UTF-8
case ${UID} in
0)
    LANG=C
    ;;
esac

# 関数をフック
autoload -Uz add-zsh-hook


## Prompt configuration
#
source ${_DOTFILES}/zshrc-prompt

## Default shell configuration
#
source ${_DOTFILES}/zshrc-shell


## Keybind configuration
#
source ${_DOTFILES}/zshrc-keybind

## Command history configuration
#
HISTFILE=${HOME}/.zsh_history
HISTSIZE=50000
SAVEHIST=50000
setopt hist_ignore_space    # ignore 
setopt hist_ignore_dups     # ignore duplication command history list
setopt share_history        # share command history data

## Completion configuration
#
source ${_DOTFILES}/zshrc-completion

## Alias configuration
#
# expand aliases before completing
#
source ${_DOTFILES}/zshrc-alias

## terminal configuration
#
source ${_DOTFILES}/zshrc-terminal

## variable configuration
#
source ${_DOTFILES}/zshrc-variables

## osx only configuration
#
if [[ $OSTYPE == darwin* ]]; then
    source ${_DOTFILES}/zshrc-osx
fi

## sudo.vim configuration
#
source ${_DOTFILES}/zshrc-sudovim


## misc configuration
#
source ${_DOTFILES}/zshrc-misc

## zplug configuration
#
source ${_DOTFILES}/zshrc-zplug

## anyenv configuration
#
source ${_DOTFILES}/zshrc-anyenv

## original command
#
source ${_DOTFILES}/zshrc-commands
