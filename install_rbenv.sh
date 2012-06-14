#!/bin/sh

cd ~/
git clone git://github.com/sstephenson/rbenv.git ~/.rbenv
mkdir ~/.rbenv/plugins/
cd ~/.rbenv/plugins/
git clone git://github.com/sstephenson/ruby-build.git ~/.rbenv/plugins/ruby-build
