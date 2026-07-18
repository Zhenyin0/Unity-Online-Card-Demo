-- player_char_entity.lua
-- 玩家角色 ORM

local ormtable = require "skynet-fly.db.orm.ormtable"
local ormadapter_mysql = require "skynet-fly.db.ormadapter.ormadapter_mysql"

local M = {}
local handle = {}

local g_ormobj = nil

function M.init()
    local adapter = ormadapter_mysql:new("orm_db")
    g_ormobj = ormtable:new("player_char")
    :int64("player_id")
    :int64("char_id")
    :int8("unlocked")
    :set_keys("player_id", "char_id")
    :set_cache(60 * 60 * 100, 500, 100000)
    :builder(adapter)
    return g_ormobj
end

-- 解锁角色
function handle.unlock_char(player_id, char_id)
    local entry = g_ormobj:get_one_entry(player_id, char_id)
    if entry then
        entry:set("unlocked", 1)
        return true
    end
    return g_ormobj:create_one_entry({
        player_id = player_id,
        char_id = char_id,
        unlocked = 1,
    })
end

-- 锁定角色
function handle.lock_char(player_id, char_id)
    local entry = g_ormobj:get_one_entry(player_id, char_id)
    if entry then
        entry:set("unlocked", 0)
        return true
    end
    return false
end

-- 获取已解锁角色列表
function handle.get_unlocked_chars(player_id)
    local all = g_ormobj:get_entry(player_id)
    local result = {}
    for _, entry in ipairs(all) do
        if entry:get("unlocked") == 1 then
            table.insert(result, entry:get("char_id"))
        end
    end
    return result
end

M.handle = handle

return M