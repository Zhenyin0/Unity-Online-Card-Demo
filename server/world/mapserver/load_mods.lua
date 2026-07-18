-- mapserver 地图服务：地图、区域、建筑、玩家地图进度
-- 数据库: map 库 (game_map_config, map_region_config, build_config, player_map, player_region_build)

local server_cfg = loadfile("../../commonlualib/common/etc/server_cfg.lua")()
local redis_cfg = loadfile("../../commonlualib/common/etc/redis_cfg.lua")()
local frpc_server_cfg = loadfile("../../commonlualib/common/etc/frpc_server_cfg.lua")()
local gate_cfg = loadfile("../../commonlualib/common/etc/gate_cfg.lua")()
local mysql_cfg = loadfile("../../commonlualib/common/etc/mysql_cfg.lua")()

return {
    logrotate_m = {
        launch_seq = 1,
        launch_num = 1,
        default_arg = {
            file_path = server_cfg.world.mapserver.logpath,
            filename = 'server.log',
            limit_size = 0,
            max_age = 7,
            max_backups = 7,
            sys_cmd = [[
                /usr/bin/pkill -HUP -f skynet.make/mapserver_config.lua\n
            ]],
        }
    },

    share_config_m = {
        launch_seq = 1000,
        launch_num = 1,
        default_arg = {
            redis = {
                rpc = redis_cfg.rpc,
                global = redis_cfg.global,
            },
            mysql = {
                orm_db = mysql_cfg.world.mapserver,
            },
            frpc_server = frpc_server_cfg.world.mapserver,
            server_cfg = server_cfg.world.mapserver,
            room_game_login = {
                wsgateconf = gate_cfg.world.mapserver,
                login_plug = "common.plug.login_plug",
            },
        }
    },

    debug_console_m = {
        launch_seq = 2000,
        launch_num = 1,
    },

    frpc_client_m = {
        launch_seq = 3000,
        launch_num = 1,
        default_arg = {
            node_map = {
                ['centerserver'] = true,
                ['hallserver'] = true,
                ['mapserver'] = true,
                ['bagserver'] = true,
                ['cardserver'] = true,
                ['fight'] = true,
                ['battle'] = true,
            },
            watch = 'redis',
        }
    },

    token_m = {
        launch_seq = 4000,
        launch_num = 1,
    },

    -- ORM: map 相关的 5 张表
    orm_table_m = {
        launch_seq = 5000,
        launch_num = 5,
        mod_args = {
            {instance_name = "game_map_config", orm_plug = "orm_entity.game_map_config_entity"},
            {instance_name = "map_region_config", orm_plug = "orm_entity.map_region_config_entity"},
            {instance_name = "build_config", orm_plug = "orm_entity.build_config_entity"},
            {instance_name = "player_map", orm_plug = "orm_entity.player_map_entity"},
            {instance_name = "player_region_build", orm_plug = "orm_entity.player_region_build_entity"},
        }
    },

    room_game_hall_m = {
        launch_seq = 6000,
        launch_num = 4,
        is_record_on = 1,
        default_arg = {
            -- hall_plug = "common.plug.hall_plug",
        }
    },

    room_game_alloc_m = {
        launch_seq = 7000,
        launch_num = 1,
        default_arg = {
            alloc_plug = "alloc.alloc_plug",
            MAX_TABLES = 5000,
        }
    },

    room_game_table_m = {
        launch_seq = 8000,
        launch_num = 1,
        default_arg = {
            table_plug = "table.table_plug",
            instance_name = "default",
            table_conf = {},
        }
    },
}