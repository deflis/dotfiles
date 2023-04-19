# Nushell Config File

# The default config record. This is where much of your global configuration is setup.
let-env config = {
  edit_mode: vi
  show_banner: false
}

alias ll = (ls -al)

source "~/.config/nushell/completions.nu"

# rtxがバグってるので、一旦コメントアウト
# https://github.com/jdxcode/rtx/pull/475
# source "~/.config/nushell/rtx.nu"
source "~/.config/nushell/oh-my-posh.nu"
source "~/.config/nushell/zoxide.nu"
