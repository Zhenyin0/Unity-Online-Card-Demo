-- mapserver 启动入口
-- 接待客户端 WS 11021，需要 room_game_login
-- Python 模板中的内部 RPC 端口 11024 在框架中用 FRPC 替代，不需要单独启动

local skynet = require "skynet"
local container_launcher = require "skynet-fly.container.container_launcher"
local log = require "skynet-fly.log"

skynet.start(function()
    local svr_name = skynet.getenv('svr_name')
    local svr_id = skynet.getenv('svr_id')
    local c_name = svr_name .. '_' .. svr_id

    skynet.setenv('error_log_path', '../../error_logs/' .. c_name)
    skynet.setenv('error_log_name', 'error.log')
    skynet.setenv('user_log_path', '../../user_logs/' .. c_name)
    skynet.setenv('user_log_name', 'user.log')

    skynet.call('.logger', 'lua', 'add_hook', 'common.log_hook')
    skynet.error("start mapserver>>>>>>>>>>>>>>>>>")

    container_launcher.run()
    skynet.uniqueservice("frpc_server")

    -- 客户端直连服务，启动 room_game_login
    skynet.uniqueservice("room_game_login")

    skynet.exit()
end)