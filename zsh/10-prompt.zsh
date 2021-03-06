## Prompt configuration
#
autoload colors
colors
setopt prompt_subst

# ブランチ名をRPROMPTで表示
autoload -Uz vcs_info

zstyle ':vcs_info:git:*' check-for-changes true
zstyle ':vcs_info:git:*' stagedstr "%F{yellow}!"
zstyle ':vcs_info:git:*' unstagedstr "%F{red}+"
zstyle ':vcs_info:*' formats "%F{green}%c%u[%b]%f"
zstyle ':vcs_info:*' actionformats '[%b|%a]'
_vcs_precmd () {
    LANG=en_US.UTF-8 vcs_info
}

local _PROMPT_YAYOI="%F{214}ζ*＇ヮ')ζ <%f"
local _PROMPT_YAYOI_ERROR="%F{red}のヮの <%f"
local _PROMPT_DIR="%F{110}%~/%f"
local _PROMPT_TIME="%F{032}[%*]%f"
local _PROMPT_RETURN="
"
case ${UID} in
    0)
        PROMPT="%{${fg[cyan]}%}$(echo ${HOST%%.*} | tr '[a-z]' '[A-Z]') %B%{${fg[red]}%}%/#%{${reset_color}%}%b "
        PROMPT2="%B%{${fg[red]}%}%_#%{${reset_color}%}%b "
        SPROMPT="%B%{${fg[red]}%}%r is correct? [n,y,a,e]:%{${reset_color}%}%b "
        ;;
    *)
        PROMPT="$_PROMPT_DIR %(?.$_PROMPT_YAYOI.$_PROMPT_YAYOI_ERROR) "'${vcs_info_msg_0_} $_PROMPT_MODE'"$_PROMPT_RETURN$_PROMPT_TIME %F{red}$%f "
        PROMPT2="%{${fg[red]}%}%_%%%{${reset_color}%} "
        SPROMPT="%{${fg[red]}%}%r is correct? [n,y,a,e]:%{${reset_color}%} "
        [ -n "${REMOTEHOST}${SSH_CONNECTION}" ] && 
        PROMPT="%{${fg[cyan]}%}$(echo ${HOST%%.*} | tr '[a-z]' '[A-Z]') ${PROMPT}"
        ;;
esac

function zle-keymap-select zle-line-init zle-line-finish
{
    case $KEYMAP in
        main|viins)
            _PROMPT_MODE="$fg[blue]INSERT$reset_color"
            ;;
        vicmd)
            _PROMPT_MODE="$fg[green]NORMAL$reset_color"
            ;;
        vivis|vivli)
            _PROMPT_MODE="$fg[magenta]VISUAL$reset_color"
            ;;
    esac
    zle reset-prompt
}

zle -N zle-line-init
zle -N zle-line-finish
zle -N zle-keymap-select
zle -N edit-command-line
RPROMPT=""

_re-prompt() {
    zle .reset-prompt
    zle .accept-line
}
zle -N accept-line _re-prompt

add-zsh-hook precmd _vcs_precmd
