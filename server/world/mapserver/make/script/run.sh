#!/bin/bash
if [ "$#" -lt 1 ]; then
	echo "arg1 [load_mods] 启动的load_mods配置"
	echo "arg2 [is_daemon] 是否守护进程运行 1是0不是 默认1"
	echo "arg3 [recordfile] 播放录像文件路径  可选"
	echo "please format make/script/run.sh load_mods.lua is_daemon"
	exit 1
fi
echo run mapserver $1 $2 $3
../../skynet_fly/skynet/3rd/lua/lua ../../skynet_fly/script/lua/write_config.lua ../../skynet_fly/ $1 $2 $3
../../skynet_fly/skynet/3rd/lua/lua ../../skynet_fly/script/lua/console.lua ../../skynet_fly/ mapserver $1 create_running_config
../../skynet_fly/skynet/3rd/lua/lua ../../skynet_fly/script/lua/console.lua ../../skynet_fly/ mapserver $1 create_load_mods_old
../../skynet_fly/skynet/skynet make/mapserver_config.lua $1
