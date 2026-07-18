-- card_template_entity.lua
-- 卡牌模板 ORM

local ormtable = require "skynet-fly.db.orm.ormtable"
local ormadapter_mysql = require "skynet-fly.db.ormadapter.ormadapter_mysql"

local M = {}
local handle = {}

local g_ormobj = nil

function M.init()
    local adapter = ormadapter_mysql:new("orm_db")
    g_ormobj = ormtable:new("card_template")
    :int64("id")
    :string64("name")
    :string1024("effect")
    :string32("category")
    :int64("mana")
    :int64("stamina")
    :int8("is_noble")
    :int8("is_all_round")
    :uint32("max_count")
    :set_keys("id")
    :set_cache(0, 500)
    :builder(adapter)
    return g_ormobj
end

M.handle = handle

return M