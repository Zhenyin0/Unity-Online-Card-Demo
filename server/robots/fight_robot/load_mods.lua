-- fight_robot 机器人测试服务
-- 对应 Python 模板: robot_server
-- 不接待客户端，内部测试用
-- 数据库: map 库 (读取 card_template, char_template)

local server_cfg = loadfile("../../commonlualib/common/etc/server_cfg.lua")()
local redis_cfg = loadfile("../../commonlualib/common/etc/redis_cfg.lua")()
local frpc_server_cfg = loadfile("../../commonlualib/common/etc/frpc_server_cfg.lua")()
local mysql_cfg = loadfile("../../commonlualib/common/etc/mysql_cfg.lua")()

return {
    logrotate_m = {
        launch_seq = 1,
        launch_num = 1,
        default_arg = {
            file_path = server_cfg.robots.fight_robot.logpath,
            filename = 'server.log',
            limit_size = 0,
            max_age = 2,
            max_backups = 7,
            sys_cmd = [[
                /usr/bin/pkill -HUP -f skynet.make/fight_robot_config.lua\n
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
                orm_db = mysql_cfg.robots.fight_robot,
            },
            frpc_server = frpc_server_cfg.robots.fight_robot,
            server_cfg = server_cfg.robots.fight_robot,
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
                ['fight'] = true,
                ['battle'] = true,
            },
            watch = 'redis',
        }
    },

    -- 机器人启动管理 (启动指定数量的机器人实例)
    robot_launch_m = {
        launch_seq = 4000,
        launch_num = 1,
        default_arg = {
            robot_num = 2,
        }
    },

    -- 机器人逻辑 (20个实例)
    robot_m = {
        launch_seq = 5000,
        launch_num = 20,
        default_arg = {
            game_name = "fight",
        }
    },
}