#!/bin/bash

env_name=$1
#关闭机器人
cd robots/fight_robot
bash make/script/stop.sh load_mods${env_name}.lua
cd ../../

#关闭游戏
cd games/fight
bash make/script/stop.sh load_mods${env_name}.lua
cd ../../

cd games/battle
bash make/script/stop.sh load_mods${env_name}.lua
cd ../../


#关闭世界服相关
cd world/cardserver
bash make/script/stop.sh load_mods${env_name}.lua
cd ../../

cd world/bagserver
bash make/script/stop.sh load_mods${env_name}.lua
cd ../../

cd world/mapserver
bash make/script/stop.sh load_mods${env_name}.lua
cd ../../
#关闭世界服相关
cd world/hallserver
bash make/script/stop.sh load_mods${env_name}_1.lua
bash make/script/stop.sh load_mods${env_name}_2.lua
cd ../../

cd world/loginserver
bash make/script/stop.sh load_mods${env_name}_1.lua
bash make/script/stop.sh load_mods${env_name}_2.lua
cd ../../

cd world/matchserver
bash make/script/stop.sh load_mods${env_name}.lua
cd ../../

cd world/centerserver
bash make/script/stop.sh load_mods${env_name}.lua
cd ../../

cd world/logserver
bash make/script/stop.sh load_mods${env_name}.lua
cd ../../