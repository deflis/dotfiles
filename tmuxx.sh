#!/bin/bash

# attach to an existing tmux session, or create one if none exist
# also set up access to the system clipboard from within tmux when possible
#
# from http://yonchu.hatenablog.com/entry/20120514/1337026014
# e.g.
# https://gist.github.com/1462391
# https://github.com/ChrisJohnsen/tmux-MacOSX-pasteboard

export SHELL=`which zsh`

if ! type tmux >/dev/null 2>&1; then
    echo 'Error: tmux command not found' 2>&1
    exit 1
fi

if [ -n "$TMUX" ]; then
    echo "Error: tmux session has been already attached" 2>&1
    exit 1
fi

tmux new-session -A -s WORK
