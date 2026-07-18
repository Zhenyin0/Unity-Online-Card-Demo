-- table_plug.lua
-- 房间插件

local ws_pbnet_util = require "skynet-fly.utils.net.ws_pbnet_util"

local M = {}

function M.init(interface_mgr)
end

M.ws_send = ws_pbnet_util.send
M.ws_broadcast = ws_pbnet_util.broadcast

function M.table_creator(table_id)
    return {
        enter = function(player_id)
        end,
        leave = function(player_id)
        end,
        disconnect = function(player_id)
        end,
        reconnect = function(player_id)
        end,
        handle = {},
    }
end

return M