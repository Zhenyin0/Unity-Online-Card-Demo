-- player_char_togame_card_entity.lua
-- 出战卡牌明细 ORM

local ormtable = require "skynet-fly.db.orm.ormtable"
local ormadapter_mysql = require "skynet-fly.db.ormadapter.ormadapter_mysql"

local M = {}
local handle = {}

local g_ormobj = nil

function M.init()
    local adapter = ormadapter_mysql:new("orm_db")
    g_ormobj = ormtable:new("player_char_togame_card")
    :int64("id")
    :int64("player_id")
    :int64("char_id")
    :int64("card_id")
    :int32("card_count")
    :set_keys("id")
    :set_cache(60 * 60 * 100, 500, 100000)
    :builder(adapter)
    return g_ormobj
end

-- 设置出战卡牌列表（先删后增）
function handle.set_togame_cards(player_id, char_id, card_list)
    -- 删除旧数据
    local all = g_ormobj:get_entry(player_id)
    for _, entry in ipairs(all) do
        if entry:get("char_id") == char_id then
            g_ormobj:delete_entry(entry:get("id"))
        end
    end

    -- 插入新数据
    for _, card in ipairs(card_list) do
        g_ormobj:create_one_entry({
            player_id = player_id,
            char_id = char_id,
            card_id = card.card_id,
            card_count = card.card_count or 1,
        })
    end
    return true
end

-- 获取出战卡牌列表
function handle.get_togame_cards(player_id, char_id)
    local all = g_ormobj:get_entry(player_id)
    local result = {}
    for _, entry in ipairs(all) do
        if entry:get("char_id") == char_id then
            table.insert(result, {
                card_id = entry:get("card_id"),
                card_count = entry:get("card_count"),
            })
        end
    end
    return result
end

M.handle = handle

return M