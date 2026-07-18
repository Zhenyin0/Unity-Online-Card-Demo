#!/bin/bash
if [ "$#" -ne 1 ]; then
	echo "please format script/check_reload.sh load_mods.lua"
	exit 1
fi
../../skynet_fly/skynet/3rd/lua/lua ../../skynet_fly/script/lua/console.lua ../../skynet_fly/ mapserver $1 check_reload | 
xargs -r -t sh make/script/reload.sh $1 
../../skynet_fly/skynet/3rd/lua/lua ../../skynet_fly/script/lua/console.lua ../../skynet_fly/ mapserver $1 check_kill_mod | 
xargs -r -t -L1 sh make/script/kill_mod.sh $1 
../../skynet_fly/skynet/3rd/lua/lua ../../skynet_fly/script/lua/console.lua ../../skynet_fly/ mapserver $1 create_load_mods_old
