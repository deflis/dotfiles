#!/bin/bash

git submodule update --init
git submodule update

ln -snf ~/dotfiles/.zsh ~/
ln -snf ~/dotfiles/.zshrc ~/
ln -snf ~/dotfiles/.vim ~/
ln -snf ~/dotfiles/.vimrc ~/
ln -snf ~/dotfiles/.gvimrc ~/
ln -snf ~/dotfiles/.tmux.conf ~/.tmux.conf
ln -snf ~/dotfiles/.tmux ~/.tmux

