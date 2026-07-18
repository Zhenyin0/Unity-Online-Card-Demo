#!/bin/bash
if [ "$#" -lt 3 ]; then
    echo "please format make/script/hotfix.sh load_mods.lua ***_m modname1|modname2|modname3 ***_m modname1|modname2|modname3"
    exit 1
fi
load_mods_name=$1
shift
../../skynet_fly/skynet/3rd/lua/lua ../../skynet_fly/script/lua/console.lua ../../skynet_fly/ centerserver ${load_mods_name} get_list | 
xargs curl -s |
xargs ../../skynet_fly/skynet/3rd/lua/lua ../../skynet_fly/script/lua/console.lua ../../skynet_fly/ centerserver ${load_mods_name} find_server_id container_mgr 2 | \
xargs -t -I {} ../../skynet_fly/skynet/3rd/lua/lua ../../skynet_fly/script/lua/console.lua ../../skynet_fly/ centerserver ${load_mods_name} hotfix {} $* | 
xargs -t curl -s | 
xargs -t ../../skynet_fly/skynet/3rd/lua/lua ../../skynet_fly/script/lua/console.lua ../../skynet_fly/ centerserver ${load_mods_name} handle_hotfix_result | xargs