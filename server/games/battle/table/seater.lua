-- seater.lua
-- 座位管理类（PVP 版本）

local skynet = require "skynet"
local SEAT_STATE = require "enum.SEAT_STATE"
local log = require "skynet-fly.log"
local timer = require "skynet-fly.timer"
local player_rpc = require "common.rpc.hallserver.player"

local setmetatable = setmetatable
local assert = assert

local M = {}
local meta = { __index = M }

function M:new()
    local t = {
        player = nil,
        state = SEAT_STATE.empty,
        seat_id = 0,
        score = 0,
        stake_item_id = 0,
        stake_item_count = 0,
        remain_total_time = 0,
        once_time = 0,
        time_obj = nil,
    }
    setmetatable(t, meta)
    return t
end

function M:enter(player_id, seat_id, stake_item_id, stake_item_count)
    local player_info = player_rpc.get_player_info(player_id)
    if not player_info then
        return false
    end
    self.player = player_info
    self.seat_id = seat_id
    self.score = player_info.rank_score or 0
    self.stake_item_id = stake_item_id or 0
    self.stake_item_count = stake_item_count or 0
    self.state = SEAT_STATE.waitting
    return true
end

function M:leave()
    self.player = nil
    self.state = SEAT_STATE.empty
    self:doing_end()
end

function M:is_empty()
    return self.state == SEAT_STATE.empty
end

function M:is_can_leave()
    return self.state ~= SEAT_STATE.playing
end

function M:get_player()
    return self.player
end

function M:get_player_id()
    return self.player and self.player.player_id or 0
end

function M:game_start()
    self.state = SEAT_STATE.playing
end

function M:game_over()
    self.state = SEAT_STATE.waitting
end

function M:get_score()
    return self.score
end

function M:add_score(num)
    if num <= 0 then return 0 end
    local player_id = self:get_player_id()
    if player_id == 0 then return 0 end
    self.score = player_rpc.change_rank_score(player_id, num)
    return num
end

function M:reduce_score(num)
    if num <= 0 then return 0 end
    local player_id = self:get_player_id()
    if player_id == 0 then return 0 end
    self.score = player_rpc.change_rank_score(player_id, -num)
    return num
end

function M:get_stake_item_id()
    return self.stake_item_id
end

function M:get_stake_item_count()
    return self.stake_item_count
end

function M:set_doing_time(total_time, one_time)
    self.remain_total_time = total_time * timer.second
    self.once_time = one_time * timer.second
end

function M:start_doing(time_out_callback, ...)
    if self.time_obj then
        self.time_obj:cancel()
        self.time_obj = nil
    end
    local expire = self.remain_total_time > self.once_time and self.once_time or self.remain_total_time
    if expire <= 0 then
        expire = self.once_time
    end
    self.time_obj = timer:new(expire, 1, time_out_callback, ...)
end

function M:get_doing_time()
    local remain_total = self.remain_total_time
    local remain_once = self.once_time
    if self.time_obj then
        remain_once = self.time_obj:remain_expire()
        remain_total = remain_total - self.once_time + remain_once
    end
    if remain_total < 0 then remain_total = 0 end
    return remain_total, remain_once
end

function M:doing_end()
    if not self.time_obj then return end
    local remain_once = self.time_obj:remain_expire()
    self.remain_total_time = self.remain_total_time - self.once_time + remain_once
    if self.remain_total_time < 0 then
        self.remain_total_time = 0
    end
    self.time_obj:cancel()
    self.time_obj = nil
end

function M:get_seat_id()
    return self.seat_id
end

return M