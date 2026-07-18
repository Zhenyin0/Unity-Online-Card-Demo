#!/bin/bash
if [ "$#" -ne 3 ]; then
	echo "please format make/script/fasttime.sh load_mods.lua 2023:10:26-19:22:50 1"
	exit 1
fi
../../skynet_fly/skynet/3rd/lua/lua ../../skynet_fly/script/lua/console.lua ../../skynet_fly/ loginserver $1 fasttime "$2" $3 | 
xargs -t curl -s 
