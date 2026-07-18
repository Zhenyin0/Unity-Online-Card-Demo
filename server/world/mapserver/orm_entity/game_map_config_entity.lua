-- game_map_config_entity.lua
-- 地图配置表 ORM

local ormtable = require "skynet-fly.db.orm.ormtable"
local ormadapter_mysql = require "skynet-fly.db.ormadapter.ormadapter_mysql"

local M = {}
local handle = {}

local g_ormobj = nil

function M.init()
    local adapter = ormadapter_mysql:new("orm_db")
    g_ormobj = ormtable:new("game_map_config")
    :int64("map_id")
    :string64("map_name")
    :table("region_ids")
    :int8("default_open")
    :uint32("sort")
    :set_keys("map_id")
    :set_cache(0, 500)
    :builder(adapter)
    return g_ormobj
end

M.handle = handle

return M