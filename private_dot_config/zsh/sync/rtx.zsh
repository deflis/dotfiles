if type rtx &>/dev/null; then
  eval "$(rtx activate zsh)"
fi
if type mise &>/dev/null; then
  eval "$(mise activate zsh --shims)" # should be first
  eval "$(mise activate zsh)"
fi
