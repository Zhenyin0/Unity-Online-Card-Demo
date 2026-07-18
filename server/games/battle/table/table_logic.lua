-- table_logic.lua
-- PVP 战斗核心逻辑（8状态回合制 + 卡牌池 + 双玩家调度）
-- 对应 Python 模板: battle_server/handlers/battle_logic.py + battle_handlers.py

local log = require "skynet-fly.log"
local errors_msg = require "common.msg.errors_msg"
local game_msg = require "msg.game_msg"
local GAME_STATE = require "enum.GAME_STATE"
local timer = require "skynet-fly.timer"
local time_util = require "skynet-fly.utils.time_util"
local env_util = require "skynet-fly.utils.env_util"
local table_util = require "skynet-fly.utils.table_util"
local skynet = require "skynet"
local json = require "cjson"
local orm_table_client = require "skynet-fly.client.orm_table_client"
local player_rpc = require "common.rpc.hallserver.player"
local bag_rpc = require "common.rpc.bagserver.bag"
local seater = require "table.seater"
local container_client = require "skynet-fly.client.container_client"

local assert = assert
local setmetatable = setmetatable
local pairs = pairs
local ipairs = ipairs
local table = table
local tonumber = tonumber
local os = os
local next = next
local math = math
local random = math.random
local tinsert = table.insert
local tremove = table.remove

container_client:register("share_config_m")

-- 回合状态定义（8状态）
local ROUND_STATUS = {
    [1] = { name = "PLAYER1_ROUND_START", desc = "玩家1回合开始", limit_time = 10 },
    [2] = { name = "PLAYER1_WAIT_ACTION", desc = "玩家1等待操作", limit_time = 60 },
    [3] = { name = "PLAYER1_ACTION_EXECUTE", desc = "玩家1操作结束", limit_time = 10 },
    [4] = { name = "PLAYER1_ROUND_END", desc = "玩家1回合结束", limit_time = 10 },
    [5] = { name = "PLAYER2_ROUND_START", desc = "玩家2回合开始", limit_time = 10 },
    [6] = { name = "PLAYER2_WAIT_ACTION", desc = "玩家2等待操作", limit_time = 60 },
    [7] = { name = "PLAYER2_ACTION_EXECUTE", desc = "玩家2操作结束", limit_time = 10 },
    [8] = { name = "PLAYER2_ROUND_END", desc = "玩家2回合结束", limit_time = 10 },
}

-- 卡牌效果类型
local CARD_EFFECT_TYPE = {
    DAMAGE = "造成",
    DEFEND = "抵抗",
    HEAL = "回复",
}

-- 卡牌三种状态
local CARD_STATUS = {
    HAND = "hand_card",
    CARRY = "carry_card",
    USED = "used_card",
}

-- 卡牌数量限制
local MAX_HAND_CARD = 5
local ROUND_MANA_LIMIT = 10
local GLOBAL_STAMINA_LIMIT = 50

local g_record_cli = orm_table_client:new("record")
local g_svr_id = env_util.get_svr_id()

local M = {}
local mata = { __index = M }

function M:new(table_id, m_interface_mgr, play_type)
    local t = {
        m_table_id = table_id,
        m_interface_mgr = m_interface_mgr,
        m_game_msg = game_msg:new(m_interface_mgr),
        m_game_state = GAME_STATE.waiting,
        m_seat_list = {},
        m_player_seat_map = {},
        m_enter_num = 0,
        m_game_seat_id_list = {},
        m_next_doing = { seat_id = 0, player_id = 0 },
        m_win_player_id = 0,
        m_play_type = play_type or 1,
        m_round_num = 0,
        m_current_status = 1,
        m_card_template_map = {},
        m_char_template_map = {},
        m_is_ended = false,
        m_round_scheduler_task = nil,
        m_record_info = {
            init_info = {},
            move_pos_list = {},
            player_info_list = {},
            win_player_id = nil,
        },
        m_chess_list = {},
        m_chess_map = {},
        m_can_move_map = {},
        m_stake_item_id = 0,
        m_stake_item_count = 0,
    }

    for i = 1, 2 do
        t.m_seat_list[i] = seater:new()
        t.m_seat_list[i]:set_doing_time(600, 60)
    end

    setmetatable(t, mata)
    return t
end

local function doing_time_out(self, seat_player)
    seat_player:doing_end()
    local seat_id = seat_player:get_seat_id()
    local win_seat_id = nil
    for _, id in pairs(self.m_game_seat_id_list) do
        if id ~= seat_id then
            win_seat_id = id
            break
        end
    end
    self:game_over(win_seat_id, "操作超时")
end

function M:get_card_template(card_id)
    if not self.m_card_template_map[card_id] then
        local cli = container_client:instance("cardserver", "card_m")
        local ret = cli:mod_call("get_card_template", card_id)
        if ret then
            self.m_card_template_map[card_id] = ret
        end
    end
    return self.m_card_template_map[card_id]
end

function M:init_card_pool(player_id, char_id, card_list)
    local player_key = "battle:player:" .. player_id .. ":" .. self.m_table_id
    local carry_card = {}
    local hand_card = {}
    local used_card = {}

    for _, card in ipairs(card_list) do
        local card_id = card.card_id or card.id or 0
        local count = card.card_count or card.count or 1
        for _ = 1, count do
            table.insert(carry_card, card_id)
        end
    end

    for i = #carry_card, 1, -1 do
        local idx = random(1, i)
        local tmp = carry_card[idx]
        carry_card[idx] = carry_card[i]
        carry_card[i] = tmp
    end
    local draw_count = math.min(MAX_HAND_CARD, #carry_card)
    for i = 1, draw_count do
        table.insert(hand_card, carry_card[1])
        tremove(carry_card, 1)
    end

    local redis = require "skynet-fly.db.redisf".instance("global")
    redis:hset(player_key, CARD_STATUS.CARRY, json.encode(carry_card))
    redis:hset(player_key, CARD_STATUS.HAND, json.encode(hand_card))
    redis:hset(player_key, CARD_STATUS.USED, json.encode(used_card))
    redis:hset(player_key, "current_char_id", char_id)
    redis:hset(player_key, "round_mana_used", 0)
    redis:hset(player_key, "total_stamina_used", 0)
    redis:expire(player_key, 7200)

    return hand_card, carry_card, used_card
}

function M:get_hand_card(player_id)
    local redis = require "skynet-fly.db.redisf".instance("global")
    local player_key = "battle:player:" .. player_id .. ":" .. self.m_table_id
    local hand_str = redis:hget(player_key, CARD_STATUS.HAND)
    if not hand_str then return {} end
    return json.decode(hand_str) or {}
end

function M:set_hand_card(player_id, hand_card)
    local redis = require "skynet-fly.db.redisf".instance("global")
    local player_key = "battle:player:" .. player_id .. ":" .. self.m_table_id
    redis:hset(player_key, CARD_STATUS.HAND, json.encode(hand_card))
end

function M:draw_card(player_id)
    local redis = require "skynet-fly.db.redisf".instance("global")
    local player_key = "battle:player:" .. player_id .. ":" .. self.m_table_id

    local carry_str = redis:hget(player_key, CARD_STATUS.CARRY)
    local hand_str = redis:hget(player_key, CARD_STATUS.HAND)
    local carry_card = carry_str and json.decode(carry_str) or {}
    local hand_card = hand_str and json.decode(hand_str) or {}

    local need_draw = MAX_HAND_CARD - #hand_card
    if need_draw > 0 and #carry_card > 0 then
        local draw_count = math.min(need_draw, #carry_card)
        for i = 1, draw_count do
            table.insert(hand_card, carry_card[1])
            tremove(carry_card, 1)
        end
        redis:hset(player_key, CARD_STATUS.CARRY, json.encode(carry_card))
        redis:hset(player_key, CARD_STATUS.HAND, json.encode(hand_card))

        local left_card = #hand_card + #carry_card
        redis:hset(player_key, "left_card", left_card)
    end

    return hand_card
end

function M:recycle_hand_card(player_id)
    local redis = require "skynet-fly.db.redisf".instance("global")
    local player_key = "battle:player:" .. player_id .. ":" .. self.m_table_id

    local hand_str = redis:hget(player_key, CARD_STATUS.HAND)
    local carry_str = redis:hget(player_key, CARD_STATUS.CARRY)
    local hand_card = hand_str and json.decode(hand_str) or {}
    local carry_card = carry_str and json.decode(carry_str) or {}

    for _, card_id in ipairs(hand_card) do
        table.insert(carry_card, card_id)
    end
    hand_card = {}

    redis:hset(player_key, CARD_STATUS.CARRY, json.encode(carry_card))
    redis:hset(player_key, CARD_STATUS.HAND, json.encode(hand_card))
    return carry_card
end

function M:settle_card_cost(player_id, mana_cost, stamina_cost)
    local redis = require "skynet-fly.db.redisf".instance("global")
    local player_key = "battle:player:" .. player_id .. ":" .. self.m_table_id

    local total_mana = tonumber(redis:hget(player_key, "mana") or 0)
    local total_stamina = tonumber(redis:hget(player_key, "stamina") or 0)
    local round_mana_used = tonumber(redis:hget(player_key, "round_mana_used") or 0)
    local total_stamina_used = tonumber(redis:hget(player_key, "total_stamina_used") or 0)

    if round_mana_used + mana_cost > ROUND_MANA_LIMIT then
        return false, "本回合魔力已达上限"
    end
    if total_stamina_used + stamina_cost > GLOBAL_STAMINA_LIMIT then
        return false, "全局体力已耗尽"
    end
    if total_mana < mana_cost then
        return false, "魔力不足"
    end
    if total_stamina < stamina_cost then
        return false, "体力不足"
    end

    redis:hset(player_key, "mana", total_mana - mana_cost)
    redis:hset(player_key, "round_mana_used", round_mana_used + mana_cost)
    redis:hset(player_key, "total_stamina_used", total_stamina_used + stamina_cost)
    return true, "消耗成功"
end

function M:mark_card_used(player_id, card_id)
    local redis = require "skynet-fly.db.redisf".instance("global")
    local player_key = "battle:player:" .. player_id .. ":" .. self.m_table_id

    local hand_str = redis:hget(player_key, CARD_STATUS.HAND)
    local used_str = redis:hget(player_key, CARD_STATUS.USED)
    local hand_card = hand_str and json.decode(hand_str) or {}
    local used_card = used_str and json.decode(used_str) or {}

    for i = #hand_card, 1, -1 do
        if hand_card[i] == card_id then
            tremove(hand_card, i)
            table.insert(used_card, card_id)
            break
        end
    end

    redis:hset(player_key, CARD_STATUS.HAND, json.encode(hand_card))
    redis:hset(player_key, CARD_STATUS.USED, json.encode(used_card))

    local carry_str = redis:hget(player_key, CARD_STATUS.CARRY)
    local carry_card = carry_str and json.decode(carry_str) or {}
    redis:hset(player_key, "left_card", #hand_card + #carry_card)
    return true
end

function M:settle_round_mana(player_id)
    local redis = require "skynet-fly.db.redisf".instance("global")
    local player_key = "battle:player:" .. player_id .. ":" .. self.m_table_id
    redis:hset(player_key, "round_mana_used", 0)
    local current_mana = tonumber(redis:hget(player_key, "mana") or 0)
    local max_mana = tonumber(redis:hget(player_key, "max_mana") or 20)
    local new_mana = math.min(current_mana + 2, max_mana)
    redis:hset(player_key, "mana", new_mana)
    return new_mana
end

function M:apply_card_effect(player_id, card_id)
    local redis = require "skynet-fly.db.redisf".instance("global")
    local player_key = "battle:player:" .. player_id .. ":" .. self.m_table_id
    local target_id = nil

    for _, seat_id in ipairs(self.m_game_seat_id_list) do
        local seat = self.m_seat_list[seat_id]
        local pid = seat:get_player_id()
        if pid ~= player_id then
            target_id = pid
            break
        end
    end

    if not target_id then return end

    local card_template = self:get_card_template(card_id)
    if not card_template then return end

    local effect = card_template.effect or ""
    local damage = 0
    local heal = 0
    local defend = 0

    local num_list = {}
    for num in string.gmatch(effect, "%d+") do
        table.insert(num_list, tonumber(num))
    end
    local effect_num = num_list[1] or 0

    if string.find(effect, "造成") then
        damage = effect_num
    elseif string.find(effect, "回复") then
        heal = effect_num
    elseif string.find(effect, "抵抗") then
        defend = effect_num
    end

    local target_key = "battle:player:" .. target_id .. ":" .. self.m_table_id
    local char_info_str = redis:hget(target_key, "char_info")
    local char_info = char_info_str and json.decode(char_info_str) or {}
    local current_char = char_info["1"] or { hp = 500, atk = 50, max_hp = 500 }

    if damage > 0 then
        local current_defend = tonumber(redis:hget(target_key, "current_defend") or 0)
        local real_damage = math.max(0, damage - current_defend)
        current_defend = math.max(0, current_defend - damage)
        redis:hset(target_key, "current_defend", current_defend)
        current_char.hp = math.max(0, current_char.hp - real_damage)
    end

    if heal > 0 then
        current_char.hp = math.min(current_char.max_hp or 500, current_char.hp + heal)
    end

    if defend > 0 then
        local current_defend = tonumber(redis:hget(player_key, "current_defend") or 0)
        redis:hset(player_key, "current_defend", current_defend + defend)
    end

    char_info["1"] = current_char
    redis:hset(target_key, "char_info", json.encode(char_info))
end

function M:check_game_end()
    local redis = require "skynet-fly.db.redisf".instance("global")
    local room_key = "battle:room:" .. self.m_table_id

    local ended = redis:hget(room_key, "game_if_end")
    if ended == "true" then
        return true, tonumber(redis:hget(room_key, "winner_id") or 0)
    end

    for _, seat_id in ipairs(self.m_game_seat_id_list) do
        local seat = self.m_seat_list[seat_id]
        local player_id = seat:get_player_id()
        if player_id > 0 then
            local player_key = "battle:player:" .. player_id .. ":" .. self.m_table_id
            local char_info_str = redis:hget(player_key, "char_info")
            if char_info_str then
                local char_info = json.decode(char_info_str) or {}
                local hp_zero = true
                for _, char in pairs(char_info) do
                    if (char.hp or 0) > 0 then
                        hp_zero = false
                        break
                    end
                end
                if hp_zero then
                    local winner_id = nil
                    for _, sid in ipairs(self.m_game_seat_id_list) do
                        if sid ~= seat_id then
                            winner_id = self.m_seat_list[sid]:get_player_id()
                            break
                        end
                    end
                    redis:hset(room_key, "game_if_end", "true")
                    redis:hset(room_key, "winner_id", winner_id or 0)
                    return true, winner_id or 0
                end
            end
        end
    end

    for _, seat_id in ipairs(self.m_game_seat_id_list) do
        local seat = self.m_seat_list[seat_id]
        local player_id = seat:get_player_id()
        if player_id > 0 then
            local player_key = "battle:player:" .. player_id .. ":" .. self.m_table_id
            local left_card = tonumber(redis:hget(player_key, "left_card") or 0)
            if left_card <= 0 then
                local winner_id = nil
                for _, sid in ipairs(self.m_game_seat_id_list) do
                    if sid ~= seat_id then
                        winner_id = self.m_seat_list[sid]:get_player_id()
                        break
                    end
                end
                redis:hset(room_key, "game_if_end", "true")
                redis:hset(room_key, "winner_id", winner_id or 0)
                return true, winner_id or 0
            end
        end
    end

    return false, 0
end

function M:update_round_status(new_status)
    local redis = require "skynet-fly.db.redisf".instance("global")
    local room_key = "battle:room:" .. self.m_table_id

    if new_status == 1 then
        self.m_round_num = self.m_round_num + 1
    end
    self.m_current_status = new_status

    local status_info = ROUND_STATUS[new_status] or {}
    redis:hset(room_key, "current_round", self.m_round_num)
    redis:hset(room_key, "round_type_number", new_status)
    redis:hset(room_key, "round_type_name", status_info.name or "")
    redis:hset(room_key, "round_limit_time", status_info.limit_time or 10)
    redis:hset(room_key, "round_start_time", os.time())

    return status_info
end

function M:start_round_scheduler()
    if self.m_round_scheduler_task then return end
    self.m_round_scheduler_task = skynet.fork(function()
        while not self.m_is_ended do
            local is_end, winner_id = self:check_game_end()
            if is_end then
                self:game_over(winner_id, "战斗结束")
                break
            end

            local redis = require "skynet-fly.db.redisf".instance("global")
            local room_key = "battle:room:" .. self.m_table_id
            local current_status = tonumber(redis:hget(room_key, "round_type_number") or 1)
            local limit_time = tonumber(redis:hget(room_key, "round_limit_time") or 10)
            local start_time = tonumber(redis:hget(room_key, "round_start_time") or os.time())

            local now = os.time()
            if now - start_time < limit_time then
                skynet.sleep(10)
                goto continue
            end

            local next_status = current_status + 1
            if next_status > 8 then
                next_status = 1
            end

            local player1_id = 0
            local player2_id = 0
            for _, sid in ipairs(self.m_game_seat_id_list) do
                if player1_id == 0 then
                    player1_id = self.m_seat_list[sid]:get_player_id()
                else
                    player2_id = self.m_seat_list[sid]:get_player_id()
                end
            end

            if current_status == 1 then
                self:draw_card(player1_id)
                self:update_round_status(2)
                self.m_next_doing.seat_id = self.m_game_seat_id_list[1]
                self.m_next_doing.player_id = player1_id
                self:send_next_doing()
                redis:hset("battle:player:" .. player1_id .. ":" .. self.m_table_id, "is_operate", "true")
                redis:hset("battle:player:" .. player2_id .. ":" .. self.m_table_id, "is_operate", "false")

            elseif current_status == 2 then
                self:recycle_hand_card(player1_id)
                self:update_round_status(3)
                self:send_next_doing()
                redis:hset("battle:player:" .. player1_id .. ":" .. self.m_table_id, "is_operate", "false")

            elseif current_status == 3 then
                self:settle_round_mana(player1_id)
                self:update_round_status(4)
                self:send_next_doing()

            elseif current_status == 4 then
                self:update_round_status(5)
                self:send_next_doing()

            elseif current_status == 5 then
                self:draw_card(player2_id)
                self:update_round_status(6)
                self.m_next_doing.seat_id = self.m_game_seat_id_list[2]
                self.m_next_doing.player_id = player2_id
                self:send_next_doing()
                redis:hset("battle:player:" .. player2_id .. ":" .. self.m_table_id, "is_operate", "true")
                redis:hset("battle:player:" .. player1_id .. ":" .. self.m_table_id, "is_operate", "false")

            elseif current_status == 6 then
                self:recycle_hand_card(player2_id)
                self:update_round_status(7)
                self:send_next_doing()
                redis:hset("battle:player:" .. player2_id .. ":" .. self.m_table_id, "is_operate", "false")

            elseif current_status == 7 then
                self:settle_round_mana(player2_id)
                self:update_round_status(8)
                self:send_next_doing()

            elseif current_status == 8 then
                self:update_round_status(1)
                self:send_next_doing()
            end

            ::continue::
        end
    end)
end

function M:game_start()
    if self.m_game_state == GAME_STATE.playing then return end
    self.m_game_state = GAME_STATE.playing
    self.m_game_seat_id_list = {}

    for seat_id, seat in ipairs(self.m_seat_list) do
        if not seat:is_empty() then
            seat:game_start()
            table.insert(self.m_game_seat_id_list, seat_id)
        end
    end

    local redis = require "skynet-fly.db.redisf".instance("global")
    local room_key = "battle:room:" .. self.m_table_id

    for _, seat_id in ipairs(self.m_game_seat_id_list) do
        local seat = self.m_seat_list[seat_id]
        local player_id = seat:get_player_id()
        local player_key = "battle:player:" .. player_id .. ":" .. self.m_table_id
        redis:hset(player_key, "player_id", player_id)
        redis:hset(player_key, "seat_id", seat_id)
        redis:hset(player_key, "mana", 20)
        redis:hset(player_key, "max_mana", 20)
        redis:hset(player_key, "stamina", 50)
        redis:hset(player_key, "max_stamina", 50)
        redis:hset(player_key, "left_card", 20)
        redis:hset(player_key, "char_info", json.encode({ ["1"] = { hp = 500, atk = 50, max_hp = 500 } }))
        redis:hset(player_key, "card_info", json.encode({}))
        redis:hset(player_key, "is_operate", "false")
        redis:hset(player_key, "current_defend", 0)
        redis:expire(player_key, 7200)

        local card_list = {}
        for i = 1, 10 do
            table.insert(card_list, { card_id = i, card_count = 2 })
        end
        self:init_card_pool(player_id, 1, card_list)
    end

    redis:hset(room_key, "current_round", 0)
    redis:hset(room_key, "round_type_number", 1)
    redis:hset(room_key, "round_type_name", "玩家1回合开始")
    redis:hset(room_key, "round_limit_time", 10)
    redis:hset(room_key, "round_start_time", os.time())
    redis:hset(room_key, "game_if_end", "false")
    redis:expire(room_key, 7200)

    self:start_round_scheduler()
    self:send_game_state()
end

function M:game_over(win_player_id, reason)
    if self.m_is_ended then return end
    self.m_is_ended = true
    self.m_game_state = GAME_STATE.over
    self.m_win_player_id = win_player_id or 0

    if self.m_round_scheduler_task then
        skynet.kill(self.m_round_scheduler_task)
        self.m_round_scheduler_task = nil
    end

    -- 结算押注道具
    if self.m_win_player_id > 0 then
        local win_seat = nil
        local lose_seat = nil
        for _, seat_id in ipairs(self.m_game_seat_id_list) do
            local seat = self.m_seat_list[seat_id]
            if seat:get_player_id() == self.m_win_player_id then
                win_seat = seat
            else
                lose_seat = seat
            end
        end

        if win_seat and lose_seat then
            local stake_item_id = win_seat:get_stake_item_id()
            local stake_item_count = win_seat:get_stake_item_count()
            if stake_item_id > 0 and stake_item_count > 0 then
                -- 胜者获得双倍押注（本金+奖励）
                local winner_id = win_seat:get_player_id()
                local loser_id = lose_seat:get_player_id()
                bag_rpc.add_item(winner_id, stake_item_id, stake_item_count * 2)
                bag_rpc.reduce_item(loser_id, stake_item_id, stake_item_count)
            end
        end

        -- 积分结算
        if win_seat then win_seat:add_score(10) end
        if lose_seat then lose_seat:reduce_score(5) end
    end

    -- 清理Redis
    local redis = require "skynet-fly.db.redisf".instance("global")
    local room_key = "battle:room:" .. self.m_table_id
    redis:del(room_key)
    for _, seat_id in ipairs(self.m_game_seat_id_list) do
        local seat = self.m_seat_list[seat_id]
        local player_key = "battle:player:" .. seat:get_player_id() .. ":" .. self.m_table_id
        redis:del(player_key)
    end

    self:send_game_state()
    self.m_game_msg:game_over_notice(self.m_table_id, self.m_win_player_id, 1)
    self.m_interface_mgr:kick_out_all("game over")
end

function M:send_game_state()
    local msg_body = {
        state = self.m_game_state,
        player_list = {},
        next_doing = self.m_next_doing,
        win_player_id = self.m_win_player_id,
    }

    for seat_id, seat in ipairs(self.m_seat_list) do
        local player = seat:get_player()
        if player then
            table.insert(msg_body.player_list, {
                player_id = player.player_id,
                seat_id = seat_id,
                nickname = player.nickname,
                score = seat:get_score(),
            })
        end
    end

    self.m_game_msg:game_state_res(msg_body)
end

function M:send_next_doing()
    self.m_game_msg:next_doing(self.m_next_doing)
end

function M:enter(player_id)
    if self.m_player_seat_map[player_id] then return end

    for seat_id, seat in ipairs(self.m_seat_list) do
        if seat:is_empty() then
            if not seat:enter(player_id, seat_id) then
                return
            end
            self.m_player_seat_map[player_id] = seat_id
            self.m_enter_num = self.m_enter_num + 1
            break
        end
    end

    if self.m_enter_num >= 2 and self.m_game_state == GAME_STATE.waiting then
        skynet.fork(self.game_start, self)
    end

    return self.m_player_seat_map[player_id]
end

function M:leave(player_id, reason)
    local seat_id = self.m_player_seat_map[player_id]
    if not seat_id then return end

    local seat = self.m_seat_list[seat_id]
    if not seat:is_can_leave() then
        return
    end

    seat:leave()
    self.m_enter_num = self.m_enter_num - 1
    self.m_player_seat_map[player_id] = nil

    return seat_id
end

function M:disconnect(player_id)
    if self.m_game_state == GAME_STATE.playing then
        -- 断线判负
        local winner_id = nil
        for _, seat_id in ipairs(self.m_game_seat_id_list) do
            local seat = self.m_seat_list[seat_id]
            if seat:get_player_id() ~= player_id then
                winner_id = seat:get_player_id()
                break
            end
        end
        if winner_id then
            self:game_over(winner_id, "对手断开连接")
        end
    end
end

function M:reconnect(player_id) end

function M:game_state_req(player_id)
    return self:send_game_state()
end

function M:move_req(player_id, pack_body)
    if self.m_game_state ~= GAME_STATE.playing then return end

    local seat_id = self.m_player_seat_map[player_id]
    if not seat_id then return end

    if seat_id ~= self.m_next_doing.seat_id then
        return
    end

    local redis = require "skynet-fly.db.redisf".instance("global")
    local player_key = "battle:player:" .. player_id .. ":" .. self.m_table_id
    local is_operate = redis:hget(player_key, "is_operate")
    if is_operate ~= "true" then
        return
    end

    local card_id = pack_body.card_id or 0
    if card_id <= 0 then return end

    local hand_card = self:get_hand_card(player_id)
    local found = false
    for _, cid in ipairs(hand_card) do
        if cid == card_id then
            found = true
            break
        end
    end
    if not found then return end

    local card_template = self:get_card_template(card_id)
    if not card_template then return end

    local mana_cost = card_template.mana or 0
    local stamina_cost = card_template.stamina or 0

    local ok, msg = self:settle_card_cost(player_id, mana_cost, stamina_cost)
    if not ok then return end

    self:mark_card_used(player_id, card_id)
    self:apply_card_effect(player_id, card_id)

    self.m_seat_list[seat_id]:doing_end()

    return true
end

return M