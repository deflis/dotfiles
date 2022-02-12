#!/usr/bin/env bash

cd $HOME
sh -c "$(curl -fsLS chezmoi.io/get)" -- init --apply deflis

if !(type "brew" > /dev/null 2>&1) ; then
  /bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
fi

eval "$(/home/linuxbrew/.linuxbrew/bin/brew shellenv)"
cd $(~/bin/chezmoi source-path)
brew bundle
