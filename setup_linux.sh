#!/bin/bash

bash ./setup_unix.sh

cd ~/.vim/bundle/vimproc/
make -f make_unix.mak
