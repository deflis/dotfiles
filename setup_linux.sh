#!/bin/bash

ln -snf ~/dotfiles/.zshrc ~/
ln -snf ~/dotfiles/.vim ~/
ln -snf ~/dotfiles/.vimrc ~/

cd ~/.vim/bundle/vimproc/
make -f make_unix.mak
