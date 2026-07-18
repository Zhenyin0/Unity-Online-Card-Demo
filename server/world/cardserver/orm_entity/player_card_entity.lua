-- player_card_entity.lua
-- 玩家卡牌 ORM

local ormtable = require "skynet-fly.db.orm.ormtable"
local ormadapter_mysql = require "skynet-fly.db.ormadapter.ormadapter_mysql"

local pairs = pairs

local M = {}
local handle = {}

local g_ormobj = nil

function M.init()
    local adapter = ormadapter_mysql:new("orm_db")
    g_ormobj = ormtable:new("player_card")
    :int64("player_id")
    :int64("card_id")
    :int32("count")
    :set_keys("player_id", "card_id")
    :set_cache(60 * 60 * 100, 500, 100000)
    :builder(adapter)
    return g_ormobj
end

-- 获取玩家所有卡牌
function handle.get_cards(player_id)
    return g_ormobj:get_entry(player_id)
end

-- 获取指定卡牌数量
function handle.get_card_count(player_id, card_id)
    local entry = g_ormobj:get_one_entry(player_id, card_id)
    if not entry then
        return 0
    end
    return entry:get("count")
end

-- 增加卡牌
function handle.add_card(player_id, card_id, num)
    local entry = g_ormobj:get_one_entry(player_id, card_id)
    if not entry then
        entry = g_ormobj:create_one_entry({ player_id = player_id, card_id = card_id, count = 0 })
    end
    local count = entry:get("count") + num
    entry:set("count", count)
    return count
end

-- 减少卡牌
function handle.reduce_card(player_id, card_id, num)
    local entry = g_ormobj:get_one_entry(player_id, card_id)
    if not entry then
        entry = g_ormobj:create_one_entry({ player_id = player_id, card_id = card_id, count = 0 })
    end
    local count = entry:get("count")
    if count < num then
        return nil
    end
    count = count - num
    entry:set("count", count)
    return count
end

M.handle = handle

return M