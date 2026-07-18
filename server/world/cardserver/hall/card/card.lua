-- card.lua
-- 卡牌路由/事件注册

local card_logic = require "hall.card.card_logic"
local PACK = require "common.pack_helper".PACK

local M = {}

function M.init(interface_mgr)
    card_logic.init(interface_mgr)
end

function M.on_login(player_id, is_jump_join)
    if is_jump_join then return end
    card_logic.on_login(player_id)
end

function M.on_loginout(player_id)
    card_logic.on_loginout(player_id)
end

function M.on_reconnect(player_id)
    card_logic.on_reconnect(player_id)
end

M.handle = {
    [PACK.cardserver_character.GetCharReq] = function(player_id, pack_id, pack_body)
        return card_logic.do_get_chars(player_id, pack_body)
    end,
    [PACK.cardserver_card.GetCardReq] = function(player_id, pack_id, pack_body)
        return card_logic.do_get_cards(player_id, pack_body)
    end,
    [PACK.cardserver_match.SelectToGamCardCharReq] = function(player_id, pack_id, pack_body)
        return card_logic.do_select_togame(player_id, pack_body)
    end,
    [PACK.cardserver_match.GetTogameReq] = function(player_id, pack_id, pack_body)
        return card_logic.do_get_togame(player_id, pack_body)
    end,
}

local CMD = {}

function CMD.unlock_char(player_id, char_id)
    return card_logic.cmd_unlock_char(player_id, char_id)
end

function CMD.add_card(player_id, card_id, num)
    return card_logic.cmd_add_card(player_id, card_id, num)
end

function CMD.get_card_template(card_id)
    return card_logic.cmd_get_card_template(card_id)
end

function CMD.get_char_template(char_id)
    return card_logic.cmd_get_char_template(char_id)
end

M.register_cmd = CMD

return M