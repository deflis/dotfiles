set nocompatible
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

if has('win32')
    set runtimepath^=$HOME/.vim
    set runtimepath+=$HOME/.vim/after
endif

if has('vim_starting')
    set runtimepath+=~/.vim/bundle/neobundle.vim/
    call neobundle#rc(expand('~/.vim/bundle/'))
endif

source ~/dotfiles/vimrc-plugins

NeoBundleCheck

filetype plugin on
filetype indent on

if has('lua') && ( (v:version == 703 && has('patch885')) || v:version >= 704 )
    source ~/dotfiles/vimrc-neocomplete
else
    source ~/dotfiles/vimrc-neocomplcache
endif
source ~/dotfiles/vimrc-omnicomplete

source ~/dotfiles/vimrc-keybind
source ~/dotfiles/vimrc-colorscheme
source ~/dotfiles/vimrc-visual

source ~/dotfiles/vimrc-quickrun
source ~/dotfiles/vimrc-plugin-config

source ~/dotfiles/vimrc-misc

