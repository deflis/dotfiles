#!/bin/sh

git pull origin master
git submodule update --init
git submodule foreach git checkout master
git submodule foreach git pull origin master
git submodule update
