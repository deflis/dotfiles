## Original command


function cddevelop {
    local _dev_directory=${HOME}/Develop
    if [[ -n "$1" ]] && [[ -d "${_dev_directory}/$1" ]]; then
        cd ${_dev_directory}/$1
    else
        anyframe-widget-cd-dir ${_dev_directory}
    fi

}

function _cddevelop {
    local _dev_directory=${HOME}/Develop
    if (( CURRENT == 2 ));then
        compadd `find ${_dev_directory}/* -type d -maxdepth 0 -exec basename '{}' ';'`
    fi
    return 1;
}
 
compdef _cddevelop cddevelop

function dotfiles-update {
    local dir
    dir=`pwd`
    cd ${HOME}/dotfiles
    git pull
    cd $dir
}

function zshrc {
    local _zshrc
    _zshrc=${HOME}/dotfiles/zshrc
    case "$1" in
    reload)
        echo "RELOAD"
        source ${_zshrc}
        ;;
    restart)
        echo "RESTART"
        exec $SHELL -l
        ;;
    edit)
        if [ -z $2 ]; then
            _$EDITOR ${_zshrc}
        else
            $EDITOR ${_zshrc}-$2
        fi
        ;;
    update)
        dotfiles-update
        ;;
    prompt)
        PROMPT="$ "
        ;;
    zcompile)
        zcompile ${_DOTFILES}/.zshrc
        zcompile ${_DOTFILES}/zshrc
        for file in ${_DOTFILES}/zshrc-* ${_DOTFILES}/zsh/**/*.zsh ; do
            zcompile $file
        done
        ;;
    zcompile-clear)
        rm ${_DOTFILES}/*.zwc
        rm ${_DOTFILES}/**/*.zwc
        ;;
    esac
} 

function _zshrc {
    local _zshrc
    _zshrc=${HOME}/dotfiles/zshrc

    if (( CURRENT == 2 )); then
        cmds=('reload' 'restart' 'edit' 'update')
        _describe -t commands "subcommand" cmds
    elif (( CURRENT == 3 )); then
        case "$words[2]" in
        edit)
            compadd `find ${_zshrc}-* -maxdepth 0 -exec basename '{}' ';' | sed -e 's/^zshrc-//'`
        esac
    fi
    return 1;
}
 
compdef _zshrc zshrc

function ptvim () {
  if [ -x "`which pt`" ]; then
    vim $(pt $@ | peco --query "$LBUFFER" | awk -F : '{print "-c " $2 " " $1}')
  else
    echo 'no pt'
  fi
}
