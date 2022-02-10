#!/usr/bin/env bash

sh -c "$(curl -fsLS chezmoi.io/get)" -- init --apply deflis

if !(type "brew" > /dev/null 2>&1) ; then
  /bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
fi

brew bundle
