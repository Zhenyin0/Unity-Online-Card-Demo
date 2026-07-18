-- hall.lua
-- 地图模块入口，加载所有子模块

local pack_helper = require "common.pack_helper"
local pb_netpack = require "skynet-fly.netpack.pb_netpack"

do
    pb_netpack.load('../../commonlualib/protos/common')
    pb_netpack.load('../../commonlualib/protos/mapserver')
    pack_helper.set_pack_id_names()
end

return {
    require "hall.map.map",
    require "hall.region.region",
}