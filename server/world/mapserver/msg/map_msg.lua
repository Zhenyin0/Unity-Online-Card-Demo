-- map_msg.lua
-- 地图消息封装

local setmetatable = setmetatable
local PACK = require "common.pack_helper".PACK

local M = {}
local meta = { __index = M }

function M:new(interface_mgr)
    local t = { interface_mgr = interface_mgr }
    setmetatable(t, meta)
    return t
end

function M:map_notice(player_id, res)
    self.interface_mgr:rpc_push_msg(player_id, PACK.mapserver_map.PlayerMapNotice, res)
end

function M:region_notice(player_id, res)
    self.interface_mgr:rpc_push_msg(player_id, PACK.mapserver_region.PlayerRegionNotice, res)
end

function M:build_notice(player_id, res)
    self.interface_mgr:rpc_push_msg(player_id, PACK.mapserver_build.PlayerBuildNotice, res)
end

function M:other_map_notice(player_id, res)
    self.interface_mgr:rpc_push_msg(player_id, PACK.mapserver_other.OtherPlayerMapNotice, res)
end

return M