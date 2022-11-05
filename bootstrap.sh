#!/usr/bin/env bash

cd $HOME
sh -c "$(curl -fsLS chezmoi.io/get)" -- init --apply deflis
