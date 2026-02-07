if (( $+commands[git-wt] )); then
  eval "$(git wt --init zsh)"

  function cdw() {
    local selected=$(git-wt | tail -n +2 | fzf --height 40% --reverse --header "Select workspace" | awk '{print $2}')
    if [ -n "$selected" ]; then
      cd "$selected"
    fi
  }
fi
