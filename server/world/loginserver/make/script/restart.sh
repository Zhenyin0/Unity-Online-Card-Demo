#!/bin/bash
if [ "$#" -lt 1 ]; then
	echo "arg1 [load_mods] 启动的load_mods配置"
	echo "arg2 [is_daemon] 是否守护进程运行 1是0不是 默认1"
	echo "arg3 [recordfile] 播放录像文件路径  可选"
	echo "please format make/script/restart.sh load_mods is_daemon"
	exit 1
fi
sh make/script/stop.sh $1
sleep 1
sh make/script/run.sh $1 $2 $3
