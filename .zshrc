## load zshrc configuration file
#
source ${HOME}/dotfiles/zshrc

## load user .zshrc configuration file
#
if [ -f ${HOME}/.zshrc.mine ]; then
    source ${HOME}/.zshrc.mine
fi
