## Default shell configuration
#
# auto change directory
#
setopt auto_cd

# auto directory pushd that you can get dirs list by cd -[tab]
#
setopt auto_pushd

# command correct edition before each completion attempt
#
setopt correct

# compacked complete list display
#
setopt list_packed

# no remove postfix slash of command line
#
setopt noautoremoveslash

# no beep sound when complete list displayed
#
setopt nolistbeep

# 小文字でも大文字ディレクトリ、ファイルを補完できるようにする
zstyle ':completion:*' matcher-list 'm:{a-z}={A-Z}'

bindkey '^ ' autosuggest-accept

export FZF_DEFAULT_OPTS='--height 40% --layout=reverse'

autoload -U up-line-or-beginning-search
autoload -U down-line-or-beginning-search
zle -N up-line-or-beginning-search
zle -N down-line-or-beginning-search
bindkey "^[[A" up-line-or-beginning-search # Up
bindkey "^[[B" down-line-or-beginning-search # Down

# Ctrl+X でCopilotのexplain機能を呼び出す
# 関数定義
ghce_bindkey() {
  local cmd=$(fc -ln -1)
  gh copilot explain "$cmd"
  echo "\n"
  zle reset-prompt
}

# Create the widget
zle -N ghce_bindkey

# Bind the widget to Ctrl+X
bindkey '^X' ghce_bindkey
