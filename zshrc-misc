function gi() { curl http://gitignore.io/api/$@ ;}

function ut2date {
  /bin/date -u -r $1 +"%Y/%m/%d %H:%M:%S UTC"
  /bin/date -r $1 +"%Y/%m/%d %H:%M:%S"
}

if (( $+commands[ip] )) ; then
    ls_deprecated_message () {
        echo -n 'net-tools コマンドはもう非推奨ですよ？おじさんなんじゃないですか？ '
    }

    arp () {
        net_tools_deprecated_message
        echo 'Use `ip n`'
    }
    ifconfig () {
        net_tools_deprecated_message
        echo 'Use `ip a`, `ip link`, `ip -s link`'
    }
    iptunnel () {
        net_tools_deprecated_message
        echo 'Use `ip tunnel`'
    }
    iwconfig () {
        echo -n 'iwconfig コマンドはもう非推奨ですよ？おじさんなんじゃないですか？ '
        echo 'Use `iw`'
    }
    nameif () {
        net_tools_deprecated_message
        echo 'Use `ip link`, `ifrename`'
    }
    netstat () {
        net_tools_deprecated_message
        echo 'Use `ss`, `ip route` (for netstat -r), `ip -s link` (for netstat -i), `ip maddr` (for netstat -g)'
    }
    route () {
        net_tools_deprecated_message
        echo 'Use `ip r`'
    }
fi

if (( $+commands[dircolors] )) ; then
    eval `dircolors ~/dotfiles/utils/dircolors-solarized/dircolors.256dark`
fi

if [ -n "$LS_COLORS" ]; then
    zstyle ':completion:*' list-colors ${(s.:.)LS_COLORS}
fi
