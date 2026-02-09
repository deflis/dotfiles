if (( $+commands[git-wt] )); then
  function cdw() {
    local selected=$(git-wt | fzf --height 40% --header-lines=1 | awk '{if ($1 == "*") print $2; else print $1}')
    if [ -n "$selected" ]; then
      cd "$selected"
    fi
  }
fi
