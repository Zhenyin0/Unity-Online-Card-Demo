-- player_char_togame_entity.lua
-- 出战角色 ORM

local ormtable = require "skynet-fly.db.orm.ormtable"
local ormadapter_mysql = require "skynet-fly.db.ormadapter.ormadapter_mysql"

local M = {}
local handle = {}

local g_ormobj = nil

function M.init()
    local adapter = ormadapter_mysql:new("orm_db")
    g_ormobj = ormtable:new("player_char_togame")
    :int64("id")
    :int64("player_id")
    :int64("room_id")
    :int64("char_id")
    :uint32("carry_card_max")
    :int8("is_current")
    :int64("create_time")
    :int64("update_time")
    :set_keys("id")
    :set_cache(60 * 60 * 100, 500, 100000)
    :builder(adapter)
    return g_ormobj
end

-- 设置当前出战角色（取消之前的 current）
function handle.set_current_char(player_id, char_id, room_id)
    -- 先取消所有当前出战
    local all = g_ormobj:get_entry(player_id)
    for _, entry in ipairs(all) do
        entry:set("is_current", 0)
    end

    -- 查找或创建
    local entry = nil
    for _, e in ipairs(all) do
        if e:get("char_id") == char_id then
            entry = e
            break
        end
    end

    if not entry then
        entry = g_ormobj:create_one_entry({
            player_id = player_id,
            room_id = room_id or 0,
            char_id = char_id,
            carry_card_max = 10,
            is_current = 1,
            create_time = os.time(),
            update_time = os.time(),
        })
    else
        entry:set("is_current", 1)
        entry:set("update_time", os.time())
    end
    return entry
end

-- 获取当前出战角色
function handle.get_current_char(player_id)
    local all = g_ormobj:get_entry(player_id)
    for _, entry in ipairs(all) do
        if entry:get("is_current") == 1 then
            return entry
        end
    end
    return nil
end

M.handle = handle

return M