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

source ${_DOTFILES}/zshrc-path

# 関数をフック
autoload -Uz add-zsh-hook

### Added by Zplugin's installer
source ${_DOTFILES}/.zplugin/bin/zplugin.zsh
autoload -Uz _zplugin
(( ${+_comps} )) && _comps[zplugin]=_zplugin
### End of Zplugin's installer chunk

source ${_DOTFILES}/zshrc-zload
source ${_DOTFILES}/zshrc-zplugin
source ${_DOTFILES}/zshrc-zload-after

## Command history configuration
#
HISTFILE=${HOME}/.zsh_history
HISTSIZE=50000
SAVEHIST=50000
setopt hist_ignore_space    # ignore 
setopt hist_ignore_dups     # ignore duplication command history list
setopt share_history        # share command history data

## osx only configuration
#
if [[ $OSTYPE == darwin* ]]; then
    source ${_DOTFILES}/zshrc-osx
fi
