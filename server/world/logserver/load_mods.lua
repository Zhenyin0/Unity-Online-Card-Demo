-- logserver 日志服务：日志收集
-- 数据库: log 库 (无业务表，框架自动管理)

local server_cfg = loadfile("../../commonlualib/common/etc/server_cfg.lua")()
local redis_cfg = loadfile("../../commonlualib/common/etc/redis_cfg.lua")()
local frpc_server_cfg = loadfile("../../commonlualib/common/etc/frpc_server_cfg.lua")()
local mysql_cfg = loadfile("../../commonlualib/common/etc/mysql_cfg.lua")()

return {
    logrotate_m = {
        launch_seq = 1,
        launch_num = 1,
        default_arg = {
            file_path = server_cfg.world.logserver.logpath,
            filename = 'server.log',
            limit_size = 0,
            max_age = 7,
            max_backups = 7,
            sys_cmd = [[
                /usr/bin/pkill -HUP -f skynet.make/logserver_config.lua\n
            ]],
        }
    },

    share_config_m = {
        launch_seq = 1000,
        launch_num = 1,
        default_arg = {
            redis = {
                rpc = redis_cfg.rpc,
            },
            mysql = {
                orm_db = mysql_cfg.world.logserver,
            },
            frpc_server = frpc_server_cfg.world.logserver,
            server_cfg = server_cfg.world.logserver,
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
                ['loginserver'] = true,
                ['hallserver'] = true,
                ['mapserver'] = true,
                ['bagserver'] = true,
                ['cardserver'] = true,
                ['logserver'] = true,
                ['fight'] = true,
                ['battle'] = true,
            },
            watch = 'redis',
        }
    },

    -- 日志收集模块
    log_gather_m = {
        launch_seq = 4000,
        launch_num = 1,
        default_arg = {
            node_map = {
                ['centerserver'] = true,
                ['loginserver'] = true,
                ['hallserver'] = true,
                ['mapserver'] = true,
                ['bagserver'] = true,
                ['cardserver'] = true,
                ['fight'] = true,
                ['battle'] = true,
            },
        }
    },
}