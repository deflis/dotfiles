set nocompatible
" set tags=~/.tags
if has('gui_running') && !has('unix')
    set encoding=utf-8
endif
scriptencoding utf-8

" display
" ----------------------
" set number
" set ruler
" set cmdheight=2
" set laststatus=2
" set statusline=%<%f\ %m%r%h%w%{'['.(&fenc!=''?&fenc:&enc).']['.&ff.']'}%=%l,%c%V%8P
" set title
" set linespace=0
" set wildmenu
" set showcmd
"set textwidth=78
"set columns=100
"set lines=150

set expandtab
set tabstop=4
set softtabstop=4
set shiftwidth=4

set nocompatible               " be iMproved
filetype plugin indent off     " required!

if has('win32')
    set runtimepath^=$HOME/.vim
    set runtimepath+=$HOME/.vim/after
endif

if has('vim_starting')
    set runtimepath+=~/.vim/bundle/neobundle.vim/
    call neobundle#rc(expand('~/.vim/bundle/'))
endif
" let NeoBundle manage NeoBundle
" required! 
NeoBundle 'Shougo/neobundle.vim'
" recommended to install
NeoBundle 'Shougo/vimproc'
" after install, turn shell ~/.vim/bundle/vimproc, (n,g)make -f your_machines_makefile
NeoBundle 'Shougo/unite.vim'
NeoBundle 'Shougo/vimshell'
NeoBundle "Shougo/neocomplcache"
NeoBundle 'Shougo/neocomplcache-snippets-complete'
NeoBundle 'tsukkee/unite-tag'
NeoBundle 'Shougo/vimfiler'

NeoBundle 'mattn/zencoding-vim'
NeoBundle 'thinca/vim-quickrun'
NeoBundle 'thinca/vim-ref'

NeoBundle 'tpope/vim-markdown'
NeoBundle 'Shougo/vimproc'
NeoBundle 'sudo.vim'
NeoBundle 'vim-ruby/vim-ruby'
NeoBundle 'majutsushi/tagbar'
NeoBundle 'scrooloose/nerdtree'
NeoBundle 'TwitVim'
" NeoBundle 'violetyk/cake.vim'

" NeoBundle 'project.vim'

filetype plugin on
filetype indent on

" 起動時に引数なしならNERDtree起動
" let file_name = expand("%")
" if has('vim_starting') &&  file_name == ""
"     autocmd VimEnter * NERDTree ./
" endif


" Disable AutoComplPop.
let g:acp_enableAtStartup = 0
" Use neocomplcache.
let g:neocomplcache_enable_at_startup = 1
" Use smartcase.
let g:neocomplcache_enable_smart_case = 1
" Use camel case completion.
let g:neocomplcache_enable_camel_case_completion = 1
" Use underbar completion.
let g:neocomplcache_enable_underbar_completion = 1
" Set minimum syntax keyword length.
let g:neocomplcache_min_syntax_length = 3
let g:neocomplcache_lock_buffer_name_pattern = '\*ku\*'

" Define dictionary.
let g:neocomplcache_dictionary_filetype_lists = {
            \ 'default' : '',
            \ 'vimshell' : $HOME.'/.vimshell_hist',
            \ 'scheme' : $HOME.'/.gosh_completions'
            \ }

" Define keyword.
if !exists('g:neocomplcache_keyword_patterns')
    let g:neocomplcache_keyword_patterns = {}
endif
let g:neocomplcache_keyword_patterns['default'] = '\h\w*'

" Plugin key-mappings.
imap <C-k>     <Plug>(neocomplcache_snippets_expand)
smap <C-k>     <Plug>(neocomplcache_snippets_expand)
inoremap <expr><C-g>     neocomplcache#undo_completion()
inoremap <expr><C-l>     neocomplcache#complete_common_string()

" SuperTab like snippets behavior.
"imap <expr><TAB> neocomplcache#sources#snippets_complete#expandable() ? "\<Plug>(neocomplcache_snippets_expand)" : pumvisible() ? "\<C-n>" : "\<TAB>"

" <CR>: close popup.
inoremap <expr><CR> pumvisible() ? neocomplcache#smart_close_popup() : "\<CR>"
" Ctrl+Spaceで補完
inoremap <expr><Nul> pumvisible() ? "\<C-n>" : neocomplcache#start_manual_complete()
" Recommended key-mappings.
" <TAB>: completion.
inoremap <expr><TAB>  pumvisible() ? "\<C-n>" : "\<TAB>"
" <C-h>, <BS>: close popup and delete backword char.
inoremap <expr><C-h> neocomplcache#smart_close_popup()."\<C-h>"
inoremap <expr><BS> neocomplcache#smart_close_popup()."\<C-h>"
inoremap <expr><C-y>  neocomplcache#close_popup()
inoremap <expr><C-e>  neocomplcache#cancel_popup()

" AutoComplPop like behavior.
"let g:neocomplcache_enable_auto_select = 1

" Shell like behavior(not recommended).
"set completeopt+=longest
"let g:neocomplcache_enable_auto_select = 1
"let g:neocomplcache_disable_auto_complete = 1
"inoremap <expr><TAB>  pumvisible() ? "\<Down>" : "\<TAB>"
"inoremap <expr><CR>  neocomplcache#smart_close_popup() . "\<CR>"

" Enable omni completion.
autocmd FileType css setlocal omnifunc=csscomplete#CompleteCSS
autocmd FileType html,markdown setlocal omnifunc=htmlcomplete#CompleteTags
autocmd FileType javascript setlocal omnifunc=javascriptcomplete#CompleteJS
autocmd FileType python setlocal omnifunc=pythoncomplete#Complete
autocmd FileType xml setlocal omnifunc=xmlcomplete#CompleteTags

" Enable heavy omni completion.
if !exists('g:neocomplcache_omni_patterns')
    let g:neocomplcache_omni_patterns = {}
endif
let g:neocomplcache_omni_patterns.ruby = '[^. *\t]\.\w*\|\h\w*::'
"autocmd FileType ruby setlocal omnifunc=rubycomplete#Complete
let g:neocomplcache_omni_patterns.php = '[^. \t]->\h\w*\|\h\w*::'
let g:neocomplcache_omni_patterns.c = '\%(\.\|->\)\h\w*'
let g:neocomplcache_omni_patterns.cpp = '\h\w*\%(\.\|->\)\h\w*\|\h\w*::'

" Escで補完を消す
let &t_ti .= "\e[?7727h"
let &t_te .= "\e[?7727l"

" Escでポップアップを閉じる
noremap <special> <Esc>O[ <Esc>
noremap! <special> <Esc>O[ <Esc>
inoremap <expr><ESC>0[ pumvisible() ? neocomplcachecomplcache#smart_close_popup() . "\<ESC>" : <ESC>
inoremap <special> <C-V><Esc>O[ <C-V><Esc>

" Ctrl+HJKLでウインドウ移動
nmap <C-h> <C-w><C-h>
nmap <C-j> <C-w><C-j>
nmap <C-k> <C-w><C-k>
nmap <C-l> <C-w><C-l>

" マウス操作
set mouse=a
set ttymouse=xterm2 

set ignorecase " 検索時に大文字小文字を区別しない


function SetScreenTabName(name)
    let arg = 'k' . a:name . '\\'
    silent! exe '!echo -n "' . arg . "\""
endfunction

if &term =~ "screen"
    autocmd VimLeave * call SetScreenTabName('shell')
    autocmd BufEnter * if bufname("") !~ "^\[A-Za-z0-9\]*://" | call SetScreenTabName("%") | endif 
endif


set list
set lcs=tab:>.,trail:_,extends:¥
" 全角スペースの位置を表示
highlight JpSpace cterm=underline ctermfg=Blue guifg=Blue
au BufRead,BufNew * match JpSpace /　/
