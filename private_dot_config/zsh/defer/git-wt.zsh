if (( $+commands[git-wt] )); then
  eval "$(git wt --init zsh)"

  function cdw() {
    local selected=$(git-wt | fzf --height 40% --reverse | awk '{print $1}')
    if [ -n "$selected" ]; then
      cd "$selected"
    fi
  }
fi
