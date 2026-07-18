-- game_msg.lua
-- 战斗消息封装

local setmetatable = setmetatable
local PACK = require "common.pack_helper".PACK

local M = {}
local meta = { __index = M }

function M:new(interface_mgr)
    local t = { interface_mgr = interface_mgr }
    setmetatable(t, meta)
    return t
end

-- 推送房间状态
function M:room_state_notice(player_id, res)
    self.interface_mgr:rpc_push_msg(player_id, PACK.battleserver_room.RoomStateNotice, res)
end

-- 推送游戏状态
function M:game_state_res(game_state_res)
    self.interface_mgr:rpc_push_broad_cast(PACK.battleserver_room.GameStateRes, game_state_res)
end

-- 推送下一步操作
function M:next_doing(next_doing)
    self.interface_mgr:rpc_push_broad_cast(PACK.battleserver_room.NextDoing, next_doing)
end

-- 推送移动结果
function M:move_res(move_res)
    self.interface_mgr:rpc_push_broad_cast(PACK.battleserver_room.MoveRes, move_res)
end

-- 推送游戏结束通知
function M:game_over_notice(room_id, winner_id, is_win)
    local res = { room_id = room_id, winner_id = winner_id, is_win = is_win }
    self.interface_mgr:rpc_push_broad_cast(PACK.battleserver_room.GameOverNotice, res)
end

return M