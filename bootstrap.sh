#!/usr/bin/env bash

/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"

if [ -e /opt/homebrew/bin/brew ] ; then
  eval "$(/opt/homebrew/bin/brew shellenv)"
fi
if [ -e /home/linuxbrew/.linuxbrew/bin/brew ] ; then
  eval "$(/home/linuxbrew/.linuxbrew/bin/brew shellenv)"
fi

brew install chezmoi

cd $HOME
chezmoi init --apply deflis
