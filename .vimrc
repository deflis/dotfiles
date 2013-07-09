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

" let NeoBundle manage NeoBundle
NeoBundle 'Shougo/neobundle.vim'
" recommended to install
NeoBundle 'Shougo/vimproc', {
\   'build' : {
\       'windows' : 'echo "Sorry, cannot update vimproc binary file in Windows."',
\       'cygwin' : 'make -f make_cygwin.mak',
\       'mac' : 'make -f make_mac.mak',
\       'unix' : 'make -f make_unix.mak',
\   },
\}
" after install, turn shell ~/.vim/bundle/vimproc, (n,g)make -f your_machines_makefile
NeoBundleLazy 'Shougo/unite.vim', {
\   'autoload' : {
\       'commands' : [ "Unite" ]
\   }
\}
NeoBundleLazy 'Shougo/vimshell',{
\   'depends' : 'Shougo/vimproc',
\   'autoload' : {
\       'commands' : [
\           {
\               'name' : 'VimShell',
\               'complete' : 'customlist,vimshell#complete'
\           },
\           'VimShellExecute',
\           'VimShellInteractive',
\           'VimShellTerminal',
\           'VimShellPop',
\        ],
\       'mappings' : ['<Plug>(vimshell_switch)']
\ }
\}
NeoBundle "Shougo/neocomplcache"
" NeoBundle 'Shougo/neocomplcache-snippets-complete'
NeoBundleLazy 'tsukkee/unite-tag', {
\   'depends' : ["Shougo/unite.vim"],
\   'autoload' : {
\       'unite_sources' : 'tag',
\   }
\}
NeoBundleLazy 'Shougo/vimfiler', {
\   'depends' : 'Shougo/unite.vim',
\   'autoload' : {
\       'commands' : [
\           {
\                'name' : 'VimFiler',
\                'complete' : 'customlist,vimfiler#complete'
\           },
\           'VimFilerExplorer',
\           'Edit',
\           'Read',
\           'Source',
\           'Write'
\       ],
\       'mappings' : ['<Plug>(vimfiler_switch)'],
\   }
\}

"NeoBundleLazy 'mattn/zencoding-vim'
NeoBundle 'thinca/vim-quickrun'
NeoBundle 'thinca/vim-ref'

NeoBundle 'tpope/vim-markdown'
NeoBundleLazy 'kannokanno/previm', {
\   "autoload" : { "filetypes" : ["markdown"] }
\}
NeoBundle 'sudo.vim'
NeoBundleLazy 'nginx.vim', {
\   "autoload" : { 'filetype' : ["nginx"] }
\}
autocmd BufNewFile,BufRead /etc/nginx/conf.d/* set ft=nginx
NeoBundleLazy 'vim-ruby/vim-ruby', {
\   "autoload" : { "filetypes" : ["ruby"] }
\}
NeoBundleLazy 'majutsushi/tagbar', {
\   'autoload' : { 'commands' : [ "TagbarOpen", "TagbarToggle", "TagbarCurrentTag" ] }
\}

NeoBundleLazy 'scrooloose/nerdtree', {
\   'autoload' : { 'commands' : [ "NERDTree", "NERDTreeClose", "NERDTreeToggle", "NERDTreeFromBookmark" ], 'explorer' : 1 }
\}
" Disable netrw.vim
let g:loaded_netrwPlugin = 1
"NeoBundleLazy 'TwitVim'
"NeoBundleLazy 'dbext.vim'
NeoBundleLazy 'kchmck/vim-coffee-script', {
\   "autoload" : { 'filetype' : ["coffee"] }
\}
autocmd BufNewFile,BufRead *.coffee,*Cakefile,*.coffeekup,*.ck,*._coffee set filetype=coffee
NeoBundleLazy 'Shougo/vinarise', {
\   'autoload' : { 'commands' : [ "Vinarise" ] }
\}
NeoBundle 'kana/vim-fakeclip.git'
NeoBundle 'tpope/vim-fugitive'
NeoBundleLazy 'tyru/open-browser.vim', {
\   'autoload' : {
\       'filetypes' : ["markdown"],
\       'functions' : "OpenBrowser",
\       'commands'  : ["OpenBrowser", "OpenBrowserSearch"],
\       'mappings'  : "<Plug>(openbrowser-smart-search)"
\   },
\}

NeoBundle 'glidenote/octoeditor.vim', {
\   'autoload' : { 'commands' : [ "OctopressList", "OctopressGenerate", "OctopressDeploy", "OctopressGrep","OctopressNew" ], 'function_prefix' : 'octoeditor' }
\}
NeoBundleLazy 'glidenote/memolist.vim', {
\   'autoload' : { 'commands' : [ "MemoNew" , "MemoList", "MemoGrep" ], 'function_prefix' : 'memolist' }
\}
NeoBundle 'kien/ctrlp.vim'
NeoBundleLazy 'Kocha/vim-unite-tig', {
\   'depends' : ["Shougo/unite.vim"],
\   'autoload' : {
\       'unite_sources' : 'tig',
\   }
\}

" NeoBundle 'thinca/vim-splash'
NeoBundle 'scrooloose/syntastic'
NeoBundle 'airblade/vim-rooter'
NeoBundle 'thinca/vim-localrc'

" NeoBundle 'minibufexpl.vim'

" NeoBundle 'project.vim'

NeoBundle 'Lokaltog/powerline', {
\   'rtp' : 'powerline/bindings/vim',
\   'base' : expand('$HOME/dotfiles')
\}

NeoBundle 'altercation/vim-colors-solarized'
NeoBundle 'elzr/vim-json'
NeoBundleCheck

filetype plugin on
filetype indent on

set expandtab
set tabstop=4
set softtabstop=4
set shiftwidth=4

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
            \ 'scheme' : $HOME.'/.gosh_completions',
            \ 'php' : $HOME . '/.vim/dict/php.dict',
            \ 'ctp' : $HOME . '/.vim/dict/php.dict'
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
" let g:neocomplcache_omni_patterns.ruby = '[^. *\t]\.\w*\|\h\w*::'
"autocmd FileType ruby setlocal omnifunc=rubycomplete#Complete
let g:neocomplcache_omni_patterns.php = '[^. \t]->\h\w*\|\h\w*::'
let g:neocomplcache_omni_patterns.c = '\%(\.\|->\)\h\w*'
let g:neocomplcache_omni_patterns.cpp = '\h\w*\%(\.\|->\)\h\w*\|\h\w*::'

" Escで補完を消す
" 何のために追加しているのか思い出せないがなぜかwin32のvimコンソールだと終了時おかしいのでここを読まないように変更
if !has('windows') || has('gui_running')
    let &t_ti .= "\e[?7727h"
    let &t_te .= "\e[?7727l"
endif

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
" 大文字小文字の両方が含まれている場合は大文字小文字を区別
set smartcase
" 検索時にファイルの最後まで行ったら最初に戻る
set wrapscan
" 括弧入力時に対応する括弧を表示
set showmatch
" 行番号を表示
set nonumber


function SetScreenTabName(name)
    let arg = 'k' . a:name . '\\'
    silent! exe '!echo -n "' . arg . "\""
endfunction

if &term =~ "screen" || (executable('tmux') && $TMUX != '')
    autocmd VimLeave * call SetScreenTabName('shell')
    autocmd BufEnter * if bufname("") !~ "^\[A-Za-z0-9\]*://" | call SetScreenTabName("%") | endif 
endif


" Use clipboard register.
if has('unnamedplus')
    set clipboard& clipboard+=unnamedplus
else
    set clipboard& clipboard+=unnamed
endif

set list
set lcs=tab:>.,trail:_,extends:¥
" 全角スペースの位置を表示
highlight JpSpace cterm=underline ctermfg=Blue guifg=Blue
au BufRead,BufNew * match JpSpace /　/

""" twitvim
"let twitvim_count = 100
"nnoremap <C-t>p :<C-u>PosttoTwitter<CR>
"nnoremap <C-t><C-t><C-t> :<C-u>PosttoTwitter<CR>
"nnoremap <C-t>t :<C-u>FriendsTwitter<CR><C-w>j
"nnoremap <C-t><C-t> :<C-u>FriendsTwitter<CR>
"nnoremap <C-t>u :<C-u>UserTwitter<CR><C-w>j
"nnoremap <C-t>r :<C-u>RepliesTwitter<CR><C-w>j
"nnoremap <C-t>n :<C-u>NextTwitter<CR>

"minibufexpl
"let g:miniBufExplMapWindowNavVim=1   "hjklで移動
"let g:miniBufExplSplitBelow=0        " Put new window above
"let g:miniBufExplMapWindowNavArrows=1
"let g:miniBufExplMapCTabSwitchBufs=1
"let g:miniBufExplModSelTarget=1
"let g:miniBufExplSplitToEdge=1

" バッファを閉じる
" nnoremap <C-d> :<C-u>bd<CR>
" 次のバッファ
" nnoremap <Space> :<C-u>MBEbn<CR>
" 次のバッファ
" nnoremap <C-n> :<C-u>MBEbn<CR>
" 前のバッファ
" nnoremap <C-p> :<C-u>MBEbp<CR>



if v:version >= 703
    NeoBundle 'violetyk/cake.vim'
    let g:cakephp_enable_auto_mode = 1
endif

let g:dbext_default_type         = 'MYSQL'
let g:dbext_default_user         = 'root'
let g:dbext_default_password     = '@ask'
let g:dbext_default_host         = 'localhost'
let g:dbext_default_dbname       = ''
let g:dbext_default_buffer_lines = '20'

" Markdown
let g:quickrun_config = {
\    'markdown': {
\      'outputter': 'browser',
\      'type':
\              executable('mdown')            ? 'markdown/mdown':
\              executable('pandoc')           ? 'markdown/pandoc':
\              executable('multimarkdown')    ? 'markdown/multimarkdown':
\              executable('MultiMarkdown.pl') ? 'markdown/MultiMarkdown.pl':
\              executable('rdiscount')        ? 'markdown/rdiscount':
\              executable('bluecloth')        ? 'markdown/bluecloth':
\              executable('markdown')         ? 'markdown/markdown':
\              executable('Markdown.pl')      ? 'markdown/Markdown.pl':
\              executable('redcarpet')        ? 'markdown/redcarpet':
\              executable('kramdown')         ? 'markdown/kramdown':
\              '',
\    },
\    'markdown/mdown': {
\      'command': 'mdown',
\      'exec': '%c -i %s',
\    },
\    'markdown/multimarkdown': {
\      'command': 'multimarkdown',
\    },
\    'markdown/MultiMarkdown.pl': {
\      'command': 'MultiMarkdown.pl',
\    },
\    'markdown/rdiscount': {
\      'command': 'rdiscount',
\    },
\    'markdown/markdown': {
\      'command': 'markdown',
\    },
\    'json': {
\      'command': 'jq',
\      'exec': "%c %a %s",
\      'args': '.',
\      'outputter': 'buffer:filetype=json'
\    },
\ }
" CoffeeSctipt
let g:quickrun_config['coffee'] = {'command' : 'coffee', 'exec' : ['%c -cbp %s']}
autocmd BufWritePost *.coffee silent CoffeeMake! -cb | cwindow | redraw!

command! -nargs=? Jq call s:Jq(<f-args>)
function! s:Jq(...)
    if 0 == a:0
        let l:arg = "."
    else
        let l:arg = a:1
    endif
    call quickrun#run({"type":"json" ,"args" : l:arg})
endfunction

" MemoList
nnoremap <Leader>mf :exe "CtrlP" g:memolist_path<cr><f5>
nnoremap <Leader>mn  :MemoNew<CR>
nnoremap <Leader>ml  :MemoList<CR>
nnoremap <Leader>mg  :MemoGrep<CR>

" CtrlP
let g:ctrlp_use_migemo = 1

" NERDTree
nnoremap <Leader>nt  :NERDTreeToggle<CR>
nnoremap <Leader>ntt :NERDTreeClose<CR>:NERDTree<CR>

let s:bundle = neobundle#get("nerdtree")
function! s:bundle.hooks.on_source(bundle)
    let g:NERDTreeIgnore=['\.clean$', '\.swp$', '\.bak$', '\~$', '^\.git$', '^\.svn$', '^\.idea$', '^\.vagrant$']
    let g:NERDTreeShowHidden=1
endfunction
unlet s:bundle

" Tagbar
nnoremap <Leader>tb  :TagbarToggle<CR>
if executable('/Applications/MacVim.app/Contents/MacOS/ctags')
    let g:tagbar_ctags_bin = '/Applications/MacVim.app/Contents/MacOS/ctags'
endif

" Solarized

syntax enable
set background=dark
colorscheme solarized
let g:powerline_config_overrides={
\   'ext': {
\       'vim': {
\           'colorscheme': 'solarized',
\        },
\   },
\ }
