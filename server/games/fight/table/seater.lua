-- seater.lua
-- 座位管理类

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
        -- 操作计时相关
        remain_total_time = 0,
        once_time = 0,
        time_obj = nil,
    }
    setmetatable(t, meta)
    return t
end

-- 坐下
function M:enter(player_id, seat_id)
    local player_info = player_rpc.get_player_info(player_id)
    if not player_info then
        return false
    end
    self.player = player_info
    self.seat_id = seat_id
    self.score = player_info.rank_score or 0
    self.state = SEAT_STATE.waitting
    return true
end

-- 离开
function M:leave()
    self.player = nil
    self.state = SEAT_STATE.empty
    self:doing_end()
end

-- 是否空座位
function M:is_empty()
    return self.state == SEAT_STATE.empty
end

-- 是否可以离开
function M:is_can_leave()
    return self.state ~= SEAT_STATE.playing
end

-- 获取玩家信息
function M:get_player()
    return self.player
end

-- 获取玩家ID
function M:get_player_id()
    return self.player and self.player.player_id or 0
end

-- 游戏开始
function M:game_start()
    self.state = SEAT_STATE.playing
end

-- 游戏结束
function M:game_over()
    self.state = SEAT_STATE.waitting
end

-- 获取分数
function M:get_score()
    return self.score
end

-- 增加积分
function M:add_score(num)
    if num <= 0 then return 0 end
    local player_id = self:get_player_id()
    if player_id == 0 then return 0 end
    self.score = player_rpc.change_rank_score(player_id, num)
    return num
end

-- 减少积分
function M:reduce_score(num)
    if num <= 0 then return 0 end
    local player_id = self:get_player_id()
    if player_id == 0 then return 0 end
    self.score = player_rpc.change_rank_score(player_id, -num)
    return num
end

-- 设置操作总时长和单次时长
function M:set_doing_time(total_time, one_time)
    self.remain_total_time = total_time * timer.second
    self.once_time = one_time * timer.second
end

-- 开始操作计时
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

-- 获取操作剩余时间
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

-- 结束操作
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

-- 获取座位号
function M:get_seat_id()
    return self.seat_id
end

return M