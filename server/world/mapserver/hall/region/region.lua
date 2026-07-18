-- region.lua
-- 区域路由/事件注册

local region_logic = require "hall.region.region_logic"
local PACK = require "common.pack_helper".PACK

local M = {}

function M.init(interface_mgr)
    region_logic.init(interface_mgr)
end

function M.on_login(player_id, is_jump_join)
    if is_jump_join then return end
    region_logic.on_login(player_id)
end

function M.on_loginout(player_id) end
function M.on_reconnect(player_id) end
function M.on_disconnect(player_id) end

M.handle = {
    [PACK.mapserver_region.SelectRegionReq] = function(player_id, pack_id, pack_body)
        return region_logic.do_select_region(player_id, pack_body)
    end,
    [PACK.mapserver_region.ChangeBuildReq] = function(player_id, pack_id, pack_body)
        return region_logic.do_change_build(player_id, pack_body)
    end,
}

local CMD = {}

function CMD.get_player_region(player_id, region_id)
    return region_logic.cmd_get_player_region(player_id, region_id)
end

function CMD.clear_region(player_id, region_id, star)
    return region_logic.cmd_clear_region(player_id, region_id, star)
end

M.register_cmd = CMD

return M