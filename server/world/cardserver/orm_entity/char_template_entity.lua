-- char_template_entity.lua
-- 角色模板 ORM

local ormtable = require "skynet-fly.db.orm.ormtable"
local ormadapter_mysql = require "skynet-fly.db.ormadapter.ormadapter_mysql"

local M = {}
local handle = {}

local g_ormobj = nil

function M.init()
    local adapter = ormadapter_mysql:new("orm_db")
    g_ormobj = ormtable:new("char_template")
    :int64("id")
    :string64("name")
    :int8("star")
    :uint32("hp")
    :uint32("atk")
    :uint32("mana")
    :uint32("stamina")
    :string32("race")
    :table("own_card_ids")
    :set_keys("id")
    :set_cache(0, 500)
    :builder(adapter)
    return g_ormobj
end

M.handle = handle

return M