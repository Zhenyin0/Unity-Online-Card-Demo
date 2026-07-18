#!/bin/bash
if [ "$#" -lt 2 ]; then
    echo "please format make/script/reload.sh load_mods.lua ***_m ***_m"
    exit 1
fi
load_mods_name=$1
shift
../../skynet_fly/skynet/3rd/lua/lua ../../skynet_fly/script/lua/console.lua ../../skynet_fly/ bagserver ${load_mods_name} get_list | 
xargs curl -s |
xargs ../../skynet_fly/skynet/3rd/lua/lua ../../skynet_fly/script/lua/console.lua ../../skynet_fly/ bagserver ${load_mods_name} find_server_id container_mgr 2 | \
xargs -t -I {} ../../skynet_fly/skynet/3rd/lua/lua ../../skynet_fly/script/lua/console.lua ../../skynet_fly/ bagserver ${load_mods_name} reload {} $* | 
xargs -t curl -s | 
xargs -t ../../skynet_fly/skynet/3rd/lua/lua ../../skynet_fly/script/lua/console.lua ../../skynet_fly/ bagserver ${load_mods_name} handle_reload_result | xargs