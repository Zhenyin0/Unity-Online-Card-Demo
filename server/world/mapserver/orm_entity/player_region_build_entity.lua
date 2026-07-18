-- player_region_build_entity.lua
-- 玩家区域建筑 ORM

local ormtable = require "skynet-fly.db.orm.ormtable"
local ormadapter_mysql = require "skynet-fly.db.ormadapter.ormadapter_mysql"

local M = {}
local handle = {}

local g_ormobj = nil

function M.init()
    local adapter = ormadapter_mysql:new("orm_db")
    g_ormobj = ormtable:new("player_region_build")
    :int64("player_id")
    :int64("region_id")
    :int8("is_open")
    :int8("is_jump")
    :int8("clear_star")
    :int8("is_building")
    :int64("build_id")
    :int64("build_start_time")
    :int64("build_end_time")
    :int64("last_product_time")
    :int64("update_time")
    :set_keys("player_id", "region_id")
    :set_cache(60 * 60 * 100, 500, 100000)
    :builder(adapter)
    return g_ormobj
end

M.handle = handle

return M