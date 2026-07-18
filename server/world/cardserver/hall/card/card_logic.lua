-- card_logic.lua
-- 卡牌核心业务逻辑

local log = require "skynet-fly.log"
local card_msg = require "msg.card_msg"
local orm_table_client = require "skynet-fly.client.orm_table_client"
local state_data = require "skynet-fly.hotfix.state_data"
local errorcode = require "common.enum.errorcode"
local time_util = require "skynet-fly.utils.time_util"

local M = {}

local g_char_cli = orm_table_client:instance("player_char")
local g_card_cli = orm_table_client:instance("player_card")
local g_togame_cli = orm_table_client:instance("player_char_togame")
local g_togame_card_cli = orm_table_client:instance("player_char_togame_card")
local g_card_template_cli = orm_table_client:instance("card_template")
local g_char_template_cli = orm_table_client:instance("char_template")

local g_local_info = state_data.alloc_table("g_local_info")

function M.init(interface_mgr)
    g_local_info.card_msg = card_msg:new(interface_mgr)
end

-- 推送角色和卡牌数据
local function card_notice(player_id)
    local chars = g_char_cli:get_unlocked_chars(player_id)
    local cards = g_card_cli:get_cards(player_id)
    local char_msg = { player_id = player_id, char_id = chars }
    local card_msg_body = { card_list = {} }
    for _, entry in ipairs(cards) do
        table.insert(card_msg_body.card_list, {
            id = entry:get("card_id"),
            count = entry:get("count"),
        })
    end
    g_local_info.card_msg:char_notice(player_id, char_msg)
    g_local_info.card_msg:card_notice(player_id, card_msg_body)
end

-- 登录
function M.on_login(player_id)
    card_notice(player_id)
end

function M.on_reconnect(player_id)
    card_notice(player_id)
end

function M.on_loginout(player_id) end

-- 获取玩家角色列表
function M.do_get_chars(player_id, pack_body)
    local chars = g_char_cli:get_unlocked_chars(player_id)
    return { player_id = player_id, char_id = chars }
end

-- 获取玩家卡牌列表
function M.do_get_cards(player_id, pack_body)
    local cards = g_card_cli:get_cards(player_id)
    local card_list = {}
    for _, entry in ipairs(cards) do
        table.insert(card_list, {
            id = entry:get("card_id"),
            count = entry:get("count"),
        })
    end
    return { card_list = card_list }
end

-- 获取出战配置
function M.do_get_togame(player_id, pack_body)
    local current = g_togame_cli:get_current_char(player_id)
    if not current then
        return { charforcard_list = {} }
    end
    local char_id = current:get("char_id")
    local cards = g_togame_card_cli:get_togame_cards(player_id, char_id)
    return {
        charforcard_list = {
            {
                char_id = char_id,
                card_list = cards,
            }
        }
    }
end

-- 选择出战角色和卡牌
function M.do_select_togame(player_id, pack_body)
    local char_id = pack_body.char_id or 0
    local card_list = pack_body.card_list or {}

    if char_id <= 0 then
        return nil, errorcode.REQ_PARAM_ERR, "invalid char_id"
    end

    -- 检查角色是否已解锁
    local chars = g_char_cli:get_unlocked_chars(player_id)
    local unlocked = false
    for _, cid in ipairs(chars) do
        if cid == char_id then
            unlocked = true
            break
        end
    end
    if not unlocked then
        return nil, errorcode.REQ_PARAM_ERR, "char not unlocked"
    end

    g_togame_cli:set_current_char(player_id, char_id, 0)
    g_togame_card_cli:set_togame_cards(player_id, char_id, card_list)

    return {
        charforcard_list = {
            {
                char_id = char_id,
                card_list = card_list,
            }
        }
    }
end

-- CMD: 解锁角色
function M.cmd_unlock_char(player_id, char_id)
    return g_char_cli:unlock_char(player_id, char_id)
end

-- CMD: 增加卡牌
function M.cmd_add_card(player_id, card_id, num)
    return g_card_cli:add_card(player_id, card_id, num)
end

return M