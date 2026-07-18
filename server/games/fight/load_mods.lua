-- fight PVE战斗服务
-- 对应 Python 模板: fight_server
-- 数据库: fgo-pve (room_template, roughly_status_record, detail_status_record)
-- 注意: 卡牌模板通过 RPC 调用 cardserver 获取，不直接连接 map 库
-- 客户端 WS 端口: 11080

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
            file_path = server_cfg.games.fight.logpath,
            filename = 'server.log',
            limit_size = 0,
            max_age = 7,
            max_backups = 7,
            sys_cmd = [[
                /usr/bin/pkill -HUP -f skynet.make/fight_config.lua\n
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
                orm_db = mysql_cfg.games.fight,
            },
            frpc_server = frpc_server_cfg.games.fight,
            server_cfg = server_cfg.games.fight,
            room_game_login = {
                wsgateconf = gate_cfg.games.fight,
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
                ['cardserver'] = true,   -- 读取卡牌模板
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

    -- ORM: fgo-pve 库 3 张表
    orm_table_m = {
        launch_seq = 5000,
        launch_num = 3,
        mod_args = {
            {instance_name = "room_template", orm_plug = "orm_entity.room_template_entity"},
            {instance_name = "roughly_status_record", orm_plug = "orm_entity.roughly_status_record_entity"},
            {instance_name = "detail_status_record", orm_plug = "orm_entity.detail_status_record_entity"},
        }
    },

    room_game_hall_m = {
        launch_seq = 6000,
        launch_num = 4,
        is_record_on = 1,
        default_arg = {
            -- hall_plug 暂不填写
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
            table_conf = {
                player_num = 2,
            },
        }
    },
}