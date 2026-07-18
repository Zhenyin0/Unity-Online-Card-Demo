#!/bin/bash
if [ "$#" -ne 1 ]; then
	echo "please format make/script/check_hotfix.sh load_mods.lua"
	exit 1
fi
../../skynet_fly/skynet/3rd/lua/lua ../../skynet_fly/script/lua/console.lua ../../skynet_fly/ centerserver $1 check_hotfix | 
xargs -r -t sh make/script/hotfix.sh $1 
