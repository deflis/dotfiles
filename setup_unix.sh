#!/bin/bash

git submodule update --init
git submodule update

ln -snf ~/dotfiles/zsh ~/.zsh
ln -snf ~/dotfiles/.zshrc ~/
ln -snf ~/dotfiles/zshenv ~/.zshenv
ln -snf ~/dotfiles/nvim ~/.vim
ln -snf ~/dotfiles/.vimrc ~/
ln -snf ~/dotfiles/.gvimrc ~/
ln -snf ~/dotfiles/.tmux.conf ~/.tmux.conf
ln -snf ~/dotfiles/tmux ~/.tmux

