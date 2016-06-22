#!/bin/bash

git submodule update --init
git submodule update

cd ~/dotfiles/.vim/bundle/neobundle.vim
git checkout master
cd ~/dotfiles

ln -snf ~/dotfiles/.zsh ~/
ln -snf ~/dotfiles/.zshrc ~/
ln -snf ~/dotfiles/.vim ~/
ln -snf ~/dotfiles/.vimrc ~/
ln -snf ~/dotfiles/.gvimrc ~/
ln -snf ~/dotfiles/.tmux.conf ~/.tmux.conf
ln -snf ~/dotfiles/.tmux ~/.tmux

