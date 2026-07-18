-- map_region_config_entity.lua
-- 区域配置表 ORM

local ormtable = require "skynet-fly.db.orm.ormtable"
local ormadapter_mysql = require "skynet-fly.db.ormadapter.ormadapter_mysql"

local M = {}
local handle = {}

local g_ormobj = nil

function M.init()
    local adapter = ormadapter_mysql:new("orm_db")
    g_ormobj = ormtable:new("map_region_config")
    :int64("region_id")
    :int64("map_id")
    :int8("status")
    :string64("region_name")
    :table("enemy_config")
    :table("reward_config")
    :uint32("sort")
    :set_keys("region_id")
    :set_cache(0, 500)
    :builder(adapter)
    return g_ormobj
end

M.handle = handle

return M