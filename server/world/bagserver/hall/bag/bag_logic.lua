-- bag_logic.lua
-- 背包核心业务逻辑

local log = require "skynet-fly.log"
local bag_msg = require "msg.bag_msg"
local orm_table_client = require "skynet-fly.client.orm_table_client"
local state_data = require "skynet-fly.hotfix.state_data"
local errorcode = require "common.enum.errorcode"

local M = {}

local g_bag_entity = orm_table_client:instance("bag")
local g_local_info = state_data.alloc_table("g_local_info")

function M.init(interface_mgr)
    g_local_info.bag_msg = bag_msg:new(interface_mgr)
end

-- 推送背包数据
local function bag_notice(player_id)
    local bag_data = g_bag_entity:get_bag(player_id)
    g_local_info.bag_msg:bag_notice(player_id, { bag_list = bag_data })
end

-- 登录
function M.on_login(player_id)
    bag_notice(player_id)
end

function M.on_reconnect(player_id)
    bag_notice(player_id)
end

function M.on_loginout(player_id) end

-- 获取背包列表
function M.do_get_bag(player_id, pack_body)
    local bag_data = g_bag_entity:get_bag(player_id)
    return { bag_list = bag_data }
end

-- CMD: 增加道具（供 RPC 调用）
function M.cmd_add_item(player_id, id, num, source)
    if not id or not num or num <= 0 then
        return nil, errorcode.REQ_PARAM_ERR, "invalid param"
    end
    local count = g_bag_entity:add_item(player_id, id, num)
    bag_notice(player_id)
    return count
end

-- CMD: 减少道具（供 RPC 调用）
function M.cmd_reduce_item(player_id, id, num, source)
    if not id or not num or num <= 0 then
        return nil, errorcode.REQ_PARAM_ERR, "invalid param"
    end
    local count = g_bag_entity:reduce_item(player_id, id, num)
    if not count then
        return nil, errorcode.REQ_PARAM_ERR, "not enough"
    end
    bag_notice(player_id)
    return count
end

-- CMD: 批量增加道具（供 RPC 调用）
function M.cmd_add_item_map(player_id, item_map, source)
    if not item_map or next(item_map) == nil then
        return nil, errorcode.REQ_PARAM_ERR, "empty item_map"
    end
    local ret = g_bag_entity:add_item_map(player_id, item_map)
    bag_notice(player_id)
    return ret
end

-- CMD: 批量减少道具（供 RPC 调用）
function M.cmd_reduce_item_map(player_id, item_map, source)
    if not item_map or next(item_map) == nil then
        return nil, errorcode.REQ_PARAM_ERR, "empty item_map"
    end
    local ret = g_bag_entity:reduce_item_map(player_id, item_map)
    if not ret then
        return nil, errorcode.REQ_PARAM_ERR, "not enough"
    end
    bag_notice(player_id)
    return ret
end

return M