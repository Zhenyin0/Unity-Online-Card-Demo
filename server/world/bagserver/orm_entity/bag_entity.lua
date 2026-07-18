-- bag_entity.lua
-- bag 表 ORM 定义

local ormtable = require "skynet-fly.db.orm.ormtable"
local ormadapter_mysql = require "skynet-fly.db.ormadapter.ormadapter_mysql"

local pairs = pairs
local tinsert = table.insert

local M = {}
local handle = {}

local g_ormobj = nil

function M.init()
    local adapter = ormadapter_mysql:new("orm_db")
    g_ormobj = ormtable:new("bag")
    :int64("player_id")
    :int64("id")
    :int64("count")
    :set_keys("player_id", "id")
    :set_cache(60 * 60 * 100, 500, 100000)
    :builder(adapter)
    return g_ormobj
end

-- 获取玩家全部背包
function handle.get_bag(player_id)
    return g_ormobj:get_entry(player_id)
end

-- 获取指定道具
function handle.get_item(player_id, id)
    local entry = g_ormobj:get_one_entry(player_id, id)
    if not entry then
        return 0
    end
    return entry:get("count")
end

-- 增加道具（返回新数量）
function handle.add_item(player_id, id, num)
    local entry = g_ormobj:get_one_entry(player_id, id)
    if not entry then
        entry = g_ormobj:create_one_entry({ player_id = player_id, id = id, count = 0 })
    end
    local count = entry:get("count") + num
    entry:set("count", count)
    return count
end

-- 减少道具（返回新数量，失败返回 nil）
function handle.reduce_item(player_id, id, num)
    local entry = g_ormobj:get_one_entry(player_id, id)
    if not entry then
        entry = g_ormobj:create_one_entry({ player_id = player_id, id = id, count = 0 })
    end
    local count = entry:get("count")
    if count < num then
        return nil
    end
    count = count - num
    entry:set("count", count)
    return count
end

-- 批量增加道具（返回变更后的 map）
function handle.add_item_map(player_id, item_map)
    local ret_map = {}
    for id, num in pairs(item_map) do
        ret_map[id] = handle.add_item(player_id, id, num)
    end
    return ret_map
end

-- 批量减少道具（全成功返回 true，否则 false）
function handle.reduce_item_map(player_id, item_map)
    -- 先检查全部足够
    for id, num in pairs(item_map) do
        local entry = g_ormobj:get_one_entry(player_id, id)
        local count = entry and entry:get("count") or 0
        if count < num then
            return false
        end
    end
    -- 全部足够再执行扣减
    for id, num in pairs(item_map) do
        handle.reduce_item(player_id, id, num)
    end
    return true
end

M.handle = handle

return M