if !&compatible
  set nocompatible
endif

" set tags=~/.tags
if has('gui_running') && !has('unix')
    set encoding=utf-8
endif
let plugin_cmdex_disable = 1
scriptencoding utf-8

filetype off
filetype plugin indent off     " required!
if filereadable('/usr/local/Frameworks/Python.framework/Versions/2.7/lib/libpython2.7.dylib')
    let $PYTHON_DLL = '/usr/local/Frameworks/Python.framework/Versions/2.7/lib/libpython2.7.dylib'
endif

if has('nvim')
    if executable('~/.pyenv/versions/3.5.1/bin/python')
        let g:python3_host_prog = '~/.pyenv/versions/3.5.1/bin/python'
    endif
endif

if has('win32')
    set runtimepath^=$HOME/.vim
    set runtimepath+=$HOME/.vim/after
endif

" reset augroup
augroup MyAutoCmd
  autocmd!
augroup END

let s:cache_home = empty($XDG_CACHE_HOME) ? expand('~/.cache') : $XDG_CACHE_HOME
let s:dein_dir = s:cache_home . '/dein'
let s:dein_repo_dir = s:dein_dir . '/repos/github.com/Shougo/dein.vim'

if !isdirectory(s:dein_repo_dir)
  call system('git clone https://github.com/Shougo/dein.vim ' . shellescape(s:dein_repo_dir))
endif

let &runtimepath = s:dein_repo_dir .",". &runtimepath
let s:toml_file = '~/dotfiles/dein.toml'
let s:toml_file_lazy = '~/dotfiles/dein.lazy.toml'
if dein#load_state(s:dein_dir)
  call dein#begin(s:dein_dir, [$MYVIMRC, expand('<sfile>'), s:toml_file, s:toml_file_lazy])
  call dein#load_toml(s:toml_file, {"lazy": 0})
  call dein#load_toml(s:toml_file_lazy, {"lazy" : 1})
  call dein#end()
  call dein#save_state()
endif

if has('vim_starting') && dein#check_install()
  call dein#install()
endif

filetype plugin on
filetype indent on

"if has('lua') && ( (v:version == 703 && has('patch885')) || v:version >= 704 )
"    source ~/dotfiles/vimrc-neocomplete
"else
"    source ~/dotfiles/vimrc-neocomplcache
"endif
source ~/dotfiles/vimrc-omnicomplete

source ~/dotfiles/vimrc-keybind
source ~/dotfiles/vimrc-colorscheme
source ~/dotfiles/vimrc-visual

source ~/dotfiles/vimrc-vimshell
source ~/dotfiles/vimrc-quickrun
source ~/dotfiles/vimrc-plugin-config


source ~/dotfiles/vimrc-misc

