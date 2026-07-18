-- bag.lua
-- 背包路由/事件注册

local bag_logic = require "hall.bag.bag_logic"
local PACK = require "common.pack_helper".PACK

local M = {}

function M.init(interface_mgr)
    bag_logic.init(interface_mgr)
end

function M.on_login(player_id, is_jump_join)
    if is_jump_join then return end
    bag_logic.on_login(player_id)
end

function M.on_loginout(player_id)
    bag_logic.on_loginout(player_id)
end

function M.on_reconnect(player_id)
    bag_logic.on_reconnect(player_id)
end

M.handle = {
    [PACK.bagserver_bag.GetBagReq] = function(player_id, pack_id, pack_body)
        return bag_logic.do_get_bag(player_id, pack_body)
    end,
}

local CMD = {}

function CMD.bag_add_item(player_id, id, num, source)
    return bag_logic.cmd_add_item(player_id, id, num, source)
end

function CMD.bag_reduce_item(player_id, id, num, source)
    return bag_logic.cmd_reduce_item(player_id, id, num, source)
end

function CMD.bag_add_item_map(player_id, item_map, source)
    return bag_logic.cmd_add_item_map(player_id, item_map, source)
end

function CMD.bag_reduce_item_map(player_id, item_map, source)
    return bag_logic.cmd_reduce_item_map(player_id, item_map, source)
end

M.register_cmd = CMD

return M