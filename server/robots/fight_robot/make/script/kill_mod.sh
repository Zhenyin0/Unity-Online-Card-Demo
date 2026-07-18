#!/bin/bash
if [ "$#" -lt 2 ]; then
    echo "please format make/script/kill_mod.sh load_mods.lua ***_m ***_m"
    exit 1
fi
load_mods_name=$1
shift
../../skynet_fly/skynet/3rd/lua/lua ../../skynet_fly/script/lua/console.lua ../../skynet_fly/ fight_robot ${load_mods_name} get_list | 
xargs curl -s |
xargs ../../skynet_fly/skynet/3rd/lua/lua ../../skynet_fly/script/lua/console.lua ../../skynet_fly/ fight_robot ${load_mods_name} find_server_id container_mgr 2 | 
xargs -t ../../skynet_fly/skynet/3rd/lua/lua ../../skynet_fly/script/lua/console.lua ../../skynet_fly/ fight_robot ${load_mods_name} call kill_modules 0 $* | 
xargs -t curl -s