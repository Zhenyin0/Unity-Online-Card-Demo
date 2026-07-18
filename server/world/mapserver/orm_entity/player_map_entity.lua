-- player_map_entity.lua
-- 玩家地图进度 ORM

local ormtable = require "skynet-fly.db.orm.ormtable"
local ormadapter_mysql = require "skynet-fly.db.ormadapter.ormadapter_mysql"

local M = {}
local handle = {}

local g_ormobj = nil

function M.init()
    local adapter = ormadapter_mysql:new("orm_db")
    g_ormobj = ormtable:new("player_map")
    :int64("player_id")
    :int64("map_id")
    :int8("is_open")
    :int64("update_time")
    :set_keys("player_id", "map_id")
    :set_cache(60 * 60 * 100, 500, 100000)
    :builder(adapter)
    return g_ormobj
end

M.handle = handle

return M