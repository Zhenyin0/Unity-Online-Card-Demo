#!/bin/bash
if [ "$#" -ne 1 ]; then
	echo "please format make/script/upsharedata.sh load_mods.lua"
	exit 1
fi
load_mods_name=$1
shift
../../skynet_fly/skynet/3rd/lua/lua ../../skynet_fly/script/lua/console.lua ../../skynet_fly/ fight_robot ${load_mods_name} get_list | 
xargs curl -s |
xargs ../../skynet_fly/skynet/3rd/lua/lua ../../skynet_fly/script/lua/console.lua ../../skynet_fly/ fight_robot ${load_mods_name} find_server_id sharedata_service 2 | \
xargs -t -I {} ../../skynet_fly/skynet/3rd/lua/lua ../../skynet_fly/script/lua/console.lua ../../skynet_fly/ fight_robot ${load_mods_name} upsharedata {} | 
xargs -t curl -s | 
xargs -t ../../skynet_fly/skynet/3rd/lua/lua ../../skynet_fly/script/lua/console.lua ../../skynet_fly/ fight_robot ${load_mods_name} handle_upsharedata_result | xargs