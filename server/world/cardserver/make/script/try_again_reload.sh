#!/bin/bash
if [ "$#" -ne 1 ]; then
	echo "please format make/script/try_again_reload.sh load_mods.lua"
	exit 1
fi

if [ ! -f "./make/$1.tmp_reload_cmd.txt" ]; then
	echo "not try_reload file"
	exit 1 \n 
fi
../../skynet_fly/skynet/3rd/lua/lua ../../skynet_fly/script/lua/console.lua ../../skynet_fly/ cardserver $1 try_again_reload | 
xargs -t curl -s | 
xargs -t ../../skynet_fly/skynet/3rd/lua/lua ../../skynet_fly/script/lua/console.lua ../../skynet_fly/ cardserver $1 handle_reload_result | xargs 
