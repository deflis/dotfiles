if [ -d "$HOME/.anyenv" ]; then
    export PATH="$HOME/.anyenv/bin:$PATH"
    eval "$(anyenv init -)"

else 
    if (( $+commands[rbenv] )) ; then
        eval "$(rbenv init -)"
    fi

    if (( $+commands[pyenv] )) ; then
        # Python version management: pyenv.
        export PYENV_ROOT="$HOME/.pyenv"
        export PATH="$PYENV_ROOT/bin:$PATH"
        eval "$(pyenv init -)"
    fi
fi


if (( $+commands[rbenv] )) ; then
    function gem() {
        `rbenv which gem` $*
        if [ "$1" = "install" ] || [ "$1" = "i" ] || [ "$1" = "uninstall" ] || [ "$1" = "uni" ]
        then
            rbenv rehash
            rehash
        fi
    }
fi

if (( $+commands[phpenv] )) ; then
    function pear() {
        command pear $*
        if [ "$1" = "install" ] || [ "$1" = "i" ] || [ "$1" = "uninstall" ] || [ "$1" = "uni" ]
        then
            phpenv rehash
            rehash
        fi
    }
fi
