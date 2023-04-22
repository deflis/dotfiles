# Nushell Config File

# The default config record. This is where much of your global configuration is setup.
let-env config = {
  edit_mode: vi
  show_banner: false
}

alias ll = ls -al

source "~/.config/nushell/completions.nu"

source "~/.config/nushell/rtx.nu"
source "~/.config/nushell/fnm.nu"

source "~/.config/nushell/oh-my-posh.nu"
source "~/.config/nushell/zoxide.nu"

