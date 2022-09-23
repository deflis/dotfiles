## Command history configuration
#
HISTFILE=${HOME}/.zsh_history
HISTSIZE=50000
SAVEHIST=50000
setopt hist_ignore_space    # ignore 
setopt hist_ignore_dups     # ignore duplication command history list
setopt share_history        # share command history data
