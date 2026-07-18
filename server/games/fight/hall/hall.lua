-- hall.lua
-- 战斗模块入口

local pack_helper = require "common.pack_helper"
local pb_netpack = require "skynet-fly.netpack.pb_netpack"

do
    pb_netpack.load('../../commonlualib/protos/common')
    pb_netpack.load('../../commonlualib/protos/fightserver')
    pack_helper.set_pack_id_names()
end

return {}