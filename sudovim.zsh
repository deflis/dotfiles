sudo() {
    local args
    case $1 in
        vi|vim)
            args=()
            for arg in $@[2,-1]
            do
                if [ $arg[1] = '-' ]; then
                    args[$(( 1+$#args ))]=$arg
                else
                    args[$(( 1+$#args ))]="sudo:$arg"
                fi
            done
            command vim $args
            ;;
        *)
            command sudo $@
            ;;
    esac
}
