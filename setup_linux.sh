#!/bin/bash

git submodule update --init

ln -snf ~/dotfiles/.zsh ~/
ln -snf ~/dotfiles/.zshrc ~/
ln -snf ~/dotfiles/.vim ~/
ln -snf ~/dotfiles/.vimrc ~/

cd ~/.vim/bundle/vimproc/
make -f make_unix.mak
