
if (( $+__ZPROF )); then
    zmodload zsh/zprof && zprof
fi

typeset -U path cdpath fpath manpath ld_library_path include

## load zshrc configuration file
#
source ${HOME}/dotfiles/zshrc

## load user .zshrc configuration file
#
if [ -f ${HOME}/.zshrc.mine ]; then
    source ${HOME}/.zshrc.mine
fi


if (( $+__ZPROF )); then
    if (which zprof > /dev/null) ;then
        zprof | less
    fi
fi
