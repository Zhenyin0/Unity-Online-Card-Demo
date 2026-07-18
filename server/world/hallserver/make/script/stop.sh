#!/bin/bash
if [ "$#" -lt 1 ]; then
	echo "arg1 [load_mods] 启动的load_mods配置"
	echo "please format make/script/stop.sh load_mods.lua"
	exit 1
fi
load_mods_name=$1
if pgrep -f "skynet.make/hallserver_config.lua ${load_mods_name}" > /dev/null; then
	../../skynet_fly/skynet/3rd/lua/lua ../../skynet_fly/script/lua/console.lua ../../skynet_fly/ hallserver ${load_mods_name} get_list | 
	xargs curl -s |
	xargs ../../skynet_fly/skynet/3rd/lua/lua ../../skynet_fly/script/lua/console.lua ../../skynet_fly/ hallserver ${load_mods_name} find_server_id container_mgr 2 | 
	xargs -t ../../skynet_fly/skynet/3rd/lua/lua ../../skynet_fly/script/lua/console.lua ../../skynet_fly/ hallserver ${load_mods_name} call shutdown | 
	xargs -t curl -s
	pids=$(pgrep -f "skynet.make/hallserver_config.lua ${load_mods_name}")
	for pid in $pids; do
		kill $pid
		echo kill $pid
		wait $pid 2>/dev/null
	done
	echo kill ok
	rm -f ./make/skynet.$1.pid
	rm -f ./make/hallserver_config.lua.$1.run
	rm -f ./make/$1.old
	rm -rf ./make/module_info_$(echo "$load_mods_name" | sed 's/\.lua$//')
	rm -rf ./make/hotfix_info_$(echo "$load_mods_name" | sed 's/\.lua$//')
else
	echo not exists pid
fi
