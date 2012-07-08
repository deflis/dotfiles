#!/bin/bash

/usr/bin/ruby -e "$(/usr/bin/curl -fsSL https://raw.github.com/mxcl/homebrew/master/Library/Contributions/install_homebrew.rb)"

brew install git zsh wget

bash ./setup_unix.sh

cd ~/.vim/bundle/vimproc/
make -f make_mac.mak
