-- build_config_entity.lua
-- 建筑配置表 ORM

local ormtable = require "skynet-fly.db.orm.ormtable"
local ormadapter_mysql = require "skynet-fly.db.ormadapter.ormadapter_mysql"

local M = {}
local handle = {}

local g_ormobj = nil

function M.init()
    local adapter = ormadapter_mysql:new("orm_db")
    g_ormobj = ormtable:new("build_config")
    :int64("build_id")
    :string64("build_name")
    :table("require_items")
    :uint32("build_time")
    :table("product_items")
    :uint32("product_time")
    :set_keys("build_id")
    :set_cache(0, 500)
    :builder(adapter)
    return g_ormobj
end

M.handle = handle

return M