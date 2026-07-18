-- map.lua
-- 地图路由/事件注册

local map_logic = require "hall.map.map_logic"
local PACK = require "common.pack_helper".PACK

local M = {}

function M.init(interface_mgr)
    map_logic.init(interface_mgr)
end

function M.on_login(player_id, is_jump_join)
    if is_jump_join then return end
    map_logic.on_login(player_id)
end

function M.on_loginout(player_id) end
function M.on_reconnect(player_id) end
function M.on_disconnect(player_id) end

M.handle = {
    [PACK.mapserver_map.SelectMapReq] = function(player_id, pack_id, pack_body)
        -- 选择地图，返回地图信息
        return map_logic.get_player_maps(pack_body.player_id or player_id)
    end,
}

local CMD = {}

function CMD.get_player_map(player_id)
    return map_logic.cmd_get_player_map(player_id)
end

function CMD.unlock_map(player_id, map_id)
    return map_logic.cmd_unlock_map(player_id, map_id)
end

M.register_cmd = CMD

return M